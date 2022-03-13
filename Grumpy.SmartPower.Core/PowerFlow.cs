using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using System.Collections;

namespace Grumpy.SmartPower.Core
{
    public class PowerFlow : IPowerFlow
    {
        private readonly IHouseBatteryService _houseBatteryService;
        private readonly IList<PowerHour> _flow;
        private readonly Lazy<int> _batterySize;
        private int _position = -1;
        private bool _disposed;

        public object Current => throw new NotImplementedException();

        PowerHour IEnumerator<PowerHour>.Current => throw new NotImplementedException();

        internal PowerFlow(IHouseBatteryService houseBatteryService)
        {
            _houseBatteryService = houseBatteryService;
            _flow = new List<PowerHour>();
            _batterySize = new Lazy<int>(() => houseBatteryService.GetBatterySize());
        }

        public void Add(DateTime from, IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices)
        {
            if (from != from.ToHour())
                throw new ArgumentOutOfRangeException(nameof(from), "Must be whole hour");

            var hour = from;

            while (true)
            {
                var production = productions?.FirstOrDefault(p => p.Hour == hour)?.WattPerHour;
                var consumption = consumptions?.FirstOrDefault(p => p.Hour == hour)?.WattPerHour;
                var price = prices?.FirstOrDefault(p => p.Hour == hour)?.Price;

                if (production == null || consumption == null || price == null)
                    break;

                Add(hour, production.Value, consumption.Value, price.Value);

                hour = hour.AddHours(1);
            }
        }

        public void Add(DateTime hour, int production, int consumption, double price)
        {
            if (_flow.Any(h => h.Hour == hour))
                throw new ArgumentException(nameof(hour), "Doublicate item");

            var last = Last();

            if (last?.Hour > hour)
                throw new ArgumentException("Adding item in invalid order", nameof(hour));

            var batteryLevel = last?.BatteryLevel ?? _houseBatteryService.GetBatteryCurrent();

            _flow.Add(new PowerHour(_houseBatteryService, hour, production, consumption, price, batteryLevel));
        }

        public IEnumerable<PowerHour> All() => _flow;

        public PowerHour? Get(DateTime hour)
        {
            return _flow.FirstOrDefault(h => h.Hour == hour.ToHour());
        }

        public PowerHour? First()
        {
            return _flow.OrderBy(h => h.Hour).FirstOrDefault();
        }

        public PowerHour? Last()
        {
            return _flow.OrderByDescending(h => h.Hour).FirstOrDefault();
        }

        public int Charge(DateTime hour, int value)
        {
            var item = GetItem(hour);

            return Charge(item, value);
        }

        public int Charge(PowerHour item)
        {
            return Charge(item, item.Power);
        }

        public int Charge(PowerHour item, int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");

            var batterySize = _batterySize.Value;
            var batteryLevel = _flow.Where(h => h.Hour > item.Hour).Max(h => h.BatteryLevel as int?) ?? 0;
            int max = Math.Min(item.MaxCharge(), batterySize - batteryLevel);
            var charge = Math.Min(max, value);

            item.ChargeBattery(charge);
            AdjustBatteryLevel(charge, i => i.Hour >= item.Hour);

            return charge;
        }

        public int Discharge(DateTime hour, int value)
        {
            var item = GetItem(hour);

            return Discharge(item, value);
        }

        public int Discharge(PowerHour item)
        {
            return Discharge(item, item.Power * -1);
        }

        public int Discharge(PowerHour item, int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");

            var batteryLevel = _flow.Where(i => i.Hour > item.Hour).Min(h => h.BatteryLevel as int?) ?? _batterySize.Value;
            int max = Math.Min(item.MaxDischarge(), batteryLevel);
            var discharge = Math.Min(max, value);

            item.DischargeBattery(discharge);
            AdjustBatteryLevel(-discharge, i => i.Hour >= item.Hour);

            return discharge;
        }

        public int Move(DateTime from, DateTime to, int value)
        {
            var source = GetItem(from);
            var target = GetItem(to);

            return Move(source, target, value);
        }

        public int Move(PowerHour source, PowerHour target)
        {
            return Move(source, target, target.Power * -1);
        }

        public int Move(PowerHour source, PowerHour target, int value)
        {
            if (value < 0)
                throw new ArgumentException("Invalid value to move", nameof(value));

            if (source.Hour >= target.Hour)
                throw new ArgumentException("Source must be before target", nameof(source));

            var batterySize = _batterySize.Value;
            var batteryLevel = _flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour).Max(i => i.BatteryLevel);
            int maxCharge = Math.Min(source.MaxCharge(), batterySize - batteryLevel);
            int maxDischage = target.MaxDischarge();
            int max = Math.Min(maxCharge, maxDischage);
            var move = Math.Min(max, value);

            source.ChargeBattery(move);
            target.DischargeBattery(move);
            AdjustBatteryLevel(move, i => i.Hour >= source.Hour && i.Hour < target.Hour);

            return move;
        }

        private PowerHour GetItem(DateTime hour)
        {
            return Get(hour.ToHour()) ?? throw new ArgumentException("Item not found in flow", nameof(hour));
        }

        private void AdjustBatteryLevel(int value, Func<PowerHour, bool> predicate)
        {
            foreach (var item in _flow.Where(predicate))
                item.AdjustBatteryLevel(value);
        }

        public bool MoveNext()
        {
            _position++;

            return _position >= _flow.Count;
        }

        public void Reset()
        {
            _position = -1;
        }

        IEnumerator<PowerHour> IEnumerable<PowerHour>.GetEnumerator()
        {
            return _flow.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _flow.GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}