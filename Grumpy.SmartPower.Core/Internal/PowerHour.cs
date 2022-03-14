using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;

namespace Grumpy.SmartPower.Core
{
    public class PowerHour
    {
        public DateTime Hour { get; private set; }
        public int Production { get; private set; }
        public int Consumption { get; private set; }
        public double Price { get; private set; }
        public int BatteryLevel { get; private set; }
        public int Power { get; private set; }
        public int Charge { get; private set; }
        public int Grid { get; private set; }

        private readonly Lazy<int> _batterySize;
        private readonly Lazy<int> _inverterLimit;

        public PowerHour(IHouseBatteryService houseBatteryService, DateTime hour, int production, int consumption, double price, int batteryLevel)
        {
            _batterySize = new Lazy<int>(() => houseBatteryService.GetBatterySize());
            _inverterLimit = new Lazy<int>(() => houseBatteryService.InverterLimit());

            if (production < 0)
                throw new ArgumentOutOfRangeException(nameof(production), "Must be a positive number");
            if (consumption < 0)
                throw new ArgumentOutOfRangeException(nameof(consumption), "Must be a positive number");
            if (batteryLevel < 0 || batteryLevel > _batterySize.Value)
                throw new ArgumentOutOfRangeException(nameof(batteryLevel), "Must be between zero and BatterySize");
            if (hour != hour.ToHour())
                throw new ArgumentOutOfRangeException(nameof(hour), "Must be whole hour");

            Hour = hour;
            Production = production;
            Consumption = consumption;
            Price = price;
            BatteryLevel = batteryLevel;
            Power = production - consumption;
        }

        public int MaxCharge()
        {
            var previousBatteryLevel = GetPreviousBatteryLevel();
            var currentRemainingCapacity = _batterySize.Value - BatteryLevel - Charge;
            var previousRemainingCapacity = _batterySize.Value - previousBatteryLevel - Charge;
            var chargingCapacity = _inverterLimit.Value - Charge;

            return Math.Min(chargingCapacity, Math.Min(currentRemainingCapacity, previousRemainingCapacity));
        }

        public int MaxDischarge()
        {
            var dischargingCapacity = _inverterLimit.Value + Charge;
            var previousBatteryLevel = GetPreviousBatteryLevel();
            var possibleDischarge = Math.Max(Power * -1, 0);

            return Math.Min(dischargingCapacity, Math.Min(possibleDischarge, previousBatteryLevel));
        }

        public int ChargeBattery(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");

            var charge = Math.Min(value, MaxCharge());

            if (Power > value)
                Power -= value;
            else if (Power < 0)
                Grid -= value;
            else
            {
                Grid -= value - Power;
                Power = 0;
            }

            Charge += value;

            return charge;
        }

        public int DischargeBattery(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");

            var discharge = Math.Min(value, MaxDischarge());

            Power += discharge;
            Charge -= discharge;

            return discharge;
        }

        public void UseGrid()
        {
            Grid += Power;
            Power = 0;
        }

        public void AdjustBatteryLevel(int value)
        {
            if (BatteryLevel + value > _batterySize.Value || BatteryLevel + value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Cannot adjust BatteryLevel lower than zero or hight than BatterySize");

            BatteryLevel += value;
        }

        private int GetPreviousBatteryLevel()
        {
            return Math.Max(0, BatteryLevel - Charge);
        }
    }
}