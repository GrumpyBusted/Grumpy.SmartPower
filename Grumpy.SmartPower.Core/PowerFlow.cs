using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;

namespace Grumpy.SmartPower.Core
{
    public class PowerFlow : IPowerFlow
    {
        private readonly IList<PowerHour> _flow;
        private readonly IHouseBatteryService _houseBatteryService;

        public PowerFlow(IHouseBatteryService houseBatteryService)
        {
            _flow = new List<PowerHour>();
            _houseBatteryService = houseBatteryService;
        }

        public void Add(DateTime hour, int consumption, int production, double price)
        {
            var h = hour.ToHour();

            if (_flow.Any(i => i.Hour == h))
                throw new ArgumentException("Adding doublicate PowerHour", nameof(hour));

            var last = Last();

            if (last?.Hour > hour)
                throw new ArgumentException("Adding PowerHour in invalid order", nameof(hour));

            var batteryLevel = last?.BatteryLevel ?? _houseBatteryService.GetBatteryCurrent();

            _flow.Add(new PowerHour(_houseBatteryService)
            {
                Hour = h,
                Production = production,
                Consumption = consumption,
                Price = price,
                BatteryLevel = batteryLevel,
                Power = production - consumption
            });
        }

        public PowerHour? Get(DateTime hour)
        {
            return _flow.FirstOrDefault(i => i.Hour == hour.ToHour());
        }

        public PowerHour? First()
        {
            return _flow.OrderBy(i => i.Hour).FirstOrDefault();
        }

        public PowerHour? Last()
        {
            return _flow.OrderByDescending(i => i.Hour).FirstOrDefault();
        }

        public IEnumerable<PowerHour> All() => _flow;

        public int Charge(DateTime hour, int value)
        {
            var item = GetItem(hour);

            return Charge(item, value);
        }

        public int Charge(PowerHour item, int value)
        {
            if (value < 0)
                throw new ArgumentException("Invalid value to charge", nameof(value));

            var batterySize = _houseBatteryService.GetBatterySize();
            var batteryLevel = _flow.Where(h => h.Hour > item.Hour).Max(h => h.BatteryLevel);
            int max = Math.Min(item.MaxCharge(), batterySize - batteryLevel);
            var charge = Math.Min(max, value);

            ChargeInt(item, charge);

            foreach (var hour in _flow.Where(h => h.Hour >= item.Hour))
                hour.BatteryLevel += charge;

            return charge;
        }

        public int Discharge(DateTime hour, int value)
        {
            var item = GetItem(hour);

            return Discharge(item, value);
        }

        public int Discharge(PowerHour item, int value)
        {
            if (value < 0)
                throw new ArgumentException("Invalid value to discharge", nameof(value));

            var batteryLevel = _flow.Where(h => h.Hour > item.Hour).Min(h => h.BatteryLevel);
            int max = Math.Min(item.MaxDischarge(), batteryLevel);
            var discharge = Math.Min(max, value);

            DischargeInt(item, discharge);

            foreach (var hour in _flow.Where(h => h.Hour >= item.Hour))
                item.BatteryLevel -= discharge;

            return discharge;
        }

        public int Move(DateTime from, DateTime to, int value)
        {
            var source = GetItem(from);
            var target = GetItem(to);

            return Move(source, target, value);
        }

        public int Move(PowerHour source, PowerHour target, int value)
        {
            if (value < 0)
                throw new ArgumentException("Invalid value to move", nameof(value));

            if (source.Hour >= target.Hour)
                throw new ArgumentException("Source must be before target", nameof(source));

            var batterySize = _houseBatteryService.GetBatterySize();
            var batteryLevel = _flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour).Max(i => i.BatteryLevel);
            int maxCharge = Math.Min(source.MaxCharge(), batterySize - batteryLevel);
            int maxDischage = target.MaxDischarge();
            int max = Math.Min(maxCharge, maxDischage);
            var move = Math.Min(max, value);

            ChargeInt(source, move);
            DischargeInt(target, move);


            foreach (var item in _flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour))
                item.BatteryLevel += move;

            return move;
        }

        private static void ChargeInt(PowerHour item, int charge)
        {
            if (item.Power > charge)
                item.Power -= charge;
            else if (item.Power < 0)
                item.Grid += charge;
            else
            {
                item.Grid += charge - item.Power;
                item.Power = 0;
            }

            item.Charge += charge;
        }

        private static void DischargeInt(PowerHour item, int discharge)
        {
            item.Power += discharge;
            item.Charge -= discharge;
        }

        private PowerHour GetItem(DateTime hour)
        {
            return Get(hour) ?? throw new ArgumentException("Item not found in flow", nameof(hour));
        }
    }
}