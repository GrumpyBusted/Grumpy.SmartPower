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
                    Power = production.Value - consumption.Value
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

            return ChargeBattery(current, value);
        }

        public int ChargeBattery(PowerItem current, int value)
        {
            int max = MaxCharge(current);
            var charge = Math.Min(max, value);

            current.Charge += charge;

            foreach (var item in _flow.Where(i => i.Hour >= hour))
                item.BatteryLevel += charge;

            return charge;
        }

        public int DischargeBattery(DateTime hour, int value)
        {
            var current = GetCurrent(hour);

            return DischargeBattery(current, value);
        }

        public int DischargeBattery(PowerItem current, int value)
        {
            int max = MaxDischarge(current);
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

            return MovePower(source, target, value);
        }

        public int MovePower(PowerItem source, PowerItem target, int value)
        {
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

        public void ChargeExtraPower()
        {
            foreach (var source in _flow.Where(i => i.Power > 0).OrderBy(i => i.Price))
            {
                source.Power -= ChargeBattery(source, source.Power);
            }
        }

        public void DistributeExtraPower()
        {
            foreach (var source in _flow.Where(i => i.Power > 0).OrderBy(i => i.Hour))
            {
                foreach (var target in _flow.Where(i => i.Hour < source.Hour && MaxDischarge(i) > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
                {
                    var move = DischargeBattery(target, MaxCharge(source));

                    target.Power += move;
                    source.Power -= ChargeBattery(source, move);

                    if (source.Power <= 0)
                        break;
                }
            }
        }

        public void DistributeBatteryPower()
        {
            foreach (var target in _flow.Where(i => MaxDischarge(i) > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                target.Power += DischargeBattery(target, target.Power * -1);
            }
        }

        public void DistributeInitialBatteryPower()
        {
            var index = _flow.Where(i => MaxDischarge(i) > 0).OrderBy(i => i.Hour).FirstOrDefault();

            while (index != null)
            {
                var recharge = _flow.Where(i => i.Hour > index.Hour && i.Price < index.Price * _chargeEfficiency && MaxCharge(i) > i.OptionalRecharge).OrderBy(i => i.Hour).FirstOrDefault();

                if (recharge == null)
                {
                    index = _flow.Where(i => MaxDischarge(i) > 0 && i.Hour > index.Hour).OrderBy(i => i.Hour).FirstOrDefault();

                    continue;
                }

                var discharge = _flow.Where(i => i.Hour >= index.Hour && i.Hour < recharge.Hour && MaxDischarge(i) > 0).OrderByDescending(i => i.Price).FirstOrDefault();

                if (discharge != null)
                {
                    var move = DischargeBattery(discharge, discharge.Power * -1);
                    discharge.Power += move;
                    recharge.OptionalRecharge += move;
                }

                index = _flow.Where(i => MaxDischarge(i) > 0).OrderBy(i => i.Hour).FirstOrDefault();
            }
        }

        public double Price()
        {
            var res = (_flow.OrderBy(i => i.Hour).FirstOrDefault()?.Price ?? 0) * _houseBatteryService.GetBatteryCurrent() * -1;

            foreach (var item in _flow.OrderBy(i => i.Hour))
            {
                res += item.Price * item.Power + item.Charge - Math.Max(0, item.Production - item.Consumption);
            }

            res += _flow.OrderBy(i => i.Hour).LastOrDefault()?.Price ?? 0;

            return res;
        }

        public void ChargeFromGrid()
        {
            foreach (var target in _flow.Where(i => i.Power < 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                foreach (var source in _flow.Where(i => i.Hour < target.Hour && i.Price < target.Price * _chargeEfficiency).OrderBy(i => i.Price).ThenByDescending(i => i.Hour))
                {
                    target.Power += MovePower(source, target, target.Power * -1);

                    if (target.Power >= 0)
                        break;
                }
            }
        }

        private PowerItem GetCurrent(DateTime hour)
        {
            return Get(hour) ?? throw new NullReferenceException("Item not found in flow");
        }

        private int MaxCharge(PowerItem item)
        {
            var batteryLevel = _flow.Where(i => i.Hour >= item.Hour).Max(i => i.BatteryLevel);

            return MaxCharge(item, batteryLevel);
        }

        private int MaxCharge(PowerItem item, int batteryLevel)
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var batterySize = _houseBatteryService.GetBatterySize();
            var remainingCapacity = batterySize - batteryLevel;
            var chargingCapacity = inverterLimit - item.Charge;

            return Math.Min(chargingCapacity, remainingCapacity);
        }

        private int MaxDischarge(PowerItem item)
        {
            var batteryLevel = _flow.Where(i => i.Hour >= item.Hour).Min(i => i.BatteryLevel);

            return MaxDischarge(item, batteryLevel);
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
