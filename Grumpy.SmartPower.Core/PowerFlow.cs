using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Core
{

    public class PowerFlow : IPowerFlow
    {
        private readonly IList<PowerItem> _flow;
        private readonly IHouseBatteryService _houseBatteryService;
        private readonly double _chargeEfficiency = 0.80;

        internal PowerFlow(IHouseBatteryService houseBatteryService, IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices, DateTime from, DateTime to)
        {
            if (from.Minute > 0 || from.Second > 0 || from.Millisecond > 0)
                throw new ArgumentException("DateTime must be whole hour", nameof(from));

            _houseBatteryService = houseBatteryService;
            _flow = new List<PowerItem>();

            var batteryLevel = houseBatteryService.GetBatteryCurrent();

            for (var hour = from; hour < to; hour = hour.AddHours(1))
            {
                var production = productions?.FirstOrDefault(p => p.Hour == hour)?.WattPerHour;
                var consumption = consumptions?.FirstOrDefault(p => p.Hour == hour)?.WattPerHour;
                var price = prices?.FirstOrDefault(p => p.Hour == hour)?.Price;

                if (production == null || consumption == null || price == null)
                    break;

                _flow.Add(new PowerItem()
                {
                    Hour = hour,
                    Production = production.Value,
                    Consumption = consumption.Value,
                    Price = price.Value,
                    BatteryLevel = batteryLevel,
                    Power = production.Value - consumption.Value,
                    Charge = 0
                });
            }
        }

        public IEnumerable<PowerItem> All() => _flow;

        public PowerItem? Get(DateTime hour)
        {
            return _flow.FirstOrDefault(i => i.Hour == hour.ToHour());
        }

        public PowerItem? First()
        {
            return _flow.OrderBy(i => i.Hour).FirstOrDefault();
        }

        public int ChargeBattery(DateTime hour, int value)
        {
            var current = GetCurrent(hour);
            var batteryLevel = _flow.Where(i => i.Hour >= hour).Max(i => i.BatteryLevel);
            int max = MaxCharge(current, batteryLevel);
            var charge = Math.Min(max, value);

            current.Charge += charge;

            foreach (var item in _flow.Where(i => i.Hour >= hour))
                item.BatteryLevel += charge;

            return charge;
        }

        public int DischargeBattery(DateTime hour, int value)
        {
            var current = Get(hour) ?? throw new NullReferenceException("Item not found in flow");
            var batteryLevel = _flow.Where(i => i.Hour >= hour).Min(i => i.BatteryLevel);
            int max = MaxDischarge(current, batteryLevel);
            var discharge = Math.Min(max, value);

            current.Charge -= discharge;

            foreach (var item in _flow.Where(i => i.Hour >= hour))
                item.BatteryLevel -= discharge;

            return discharge;
        }

        public int MovePower(DateTime from, DateTime to, int value)
        {
            var source = GetCurrent(from);
            var target = GetCurrent(to);
            var batteryLevel = _flow.Where(i => i.Hour >= from && i.Hour < to).Max(i => i.BatteryLevel);
            int maxCharge = MaxCharge(source, batteryLevel);
            int maxDischage = MaxDischarge(target, maxCharge);
            int max = Math.Min(maxCharge, maxDischage);
            var move = Math.Min(max, value);

            source.Charge += move;
            target.Charge -= move;

            foreach (var item in _flow.Where(i => i.Hour >= from && i.Hour < to))
                item.BatteryLevel += move;

            return move;
        }

        public void DistributeExtraSolarPower()
        {
            foreach (var source in _flow.Where(i => i.Power > 0).OrderBy(i => i.Hour))
            {
                source.Power -= ChargeBattery(source.Hour, source.Power);

                if (source.Power <= 0)
                    continue;

                foreach (var target in _flow.Where(i => i.Hour < source.Hour && i.Power < 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
                {
                    var move = DischargeBattery(target.Hour, source.Power);

                    target.Power += move;
                    source.Power -= ChargeBattery(source.Hour, move);

                    if (source.Power <= 0)
                        break;
                }
            }
        }

        public void DistributeInitialBatteryPower()
        {
            var index = _flow.Where(i => i.Power < 0 && i.BatteryLevel > 0).FirstOrDefault();

            while (index != null)
            {
                var rechargeOption = _flow.Where(i => i.Hour > index.Hour && i.Price < index.Price * _chargeEfficiency).OrderBy(i => i.Hour).FirstOrDefault();

                if (rechargeOption == null)
                {
                    index = _flow.Where(i => i.Power < 0 && i.BatteryLevel > 0 && i.Hour > index.Hour).FirstOrDefault();

                    continue;
                }

                var discharge = _flow.Where(i => i.Hour >= index.Hour && i.Hour < rechargeOption.Hour && i.Power < 0).OrderByDescending(i => i.Price).FirstOrDefault();

                if (discharge != null)
                    discharge.Power += DischargeBattery(discharge.Hour, discharge.Power * -1);

                index = _flow.Where(i => i.Power < 0 && i.BatteryLevel > 0 && i.Hour >= index.Hour).FirstOrDefault();
            }
        }

        public void DistributeBatteryPower()
        {
            foreach (var target in _flow.Where(i => i.Power < 0 && i.BatteryLevel > 0).OrderByDescending(i => i.Price))
            {
                target.Power += DischargeBattery(target.Hour, target.Power * -1);

                var minBatteryLevel = _flow.Where(i => i.Power < 0 && i.BatteryLevel > 0).Min(i => i.BatteryLevel);

                if (minBatteryLevel <= 0)
                    break;
            }
        }

        public void ChargeFromGrid()
        {
            foreach (var target in _flow.Where(i => i.Power < 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                foreach (var source in _flow.Where(i => i.Hour < target.Hour && i.Price < target.Price * _chargeEfficiency).OrderBy(i => i.Price).ThenByDescending(i => i.Hour))
                {
                    target.Power += MovePower(source.Hour, target.Hour, target.Power * -1);

                    if (target.Power >= 0)
                        break;
                }
            }
        }

        private PowerItem GetCurrent(DateTime hour)
        {
            return Get(hour) ?? throw new NullReferenceException("Item not found in flow");
        }

        private int MaxCharge(PowerItem item, int batteryLevel)
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var batterySize = _houseBatteryService.GetBatterySize();
            var remainingCapacity = batterySize - batteryLevel;
            var chargingCapacity = inverterLimit - item.Charge;

            return Math.Min(chargingCapacity, remainingCapacity);
        }

        private int MaxDischarge(PowerItem item, int batteryLevel)
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var dischargingCapacity = inverterLimit + item.Charge;
            var maxDischarge = Math.Min(dischargingCapacity, batteryLevel);
            var possibleDischarge = Math.Max(item.Power * -1, 0);

            return Math.Min(maxDischarge, possibleDischarge);
        }
    }
}
