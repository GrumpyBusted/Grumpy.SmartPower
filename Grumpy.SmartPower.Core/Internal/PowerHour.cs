using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;

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

        public int MaxCharge(int potentialDischargeBefore = 0)
        {
            if (potentialDischargeBefore < 0 || potentialDischargeBefore > BatteryLevel)
                throw new ArgumentOutOfRangeException(nameof(potentialDischargeBefore), "Must be between zero and battery level");

            var previousBatteryLevel = GetPreviousBatteryLevel();
            var currentRemaining = _batterySize.Value - (BatteryLevel - potentialDischargeBefore);
            var previousRemaining = Math.Max(_batterySize.Value - (previousBatteryLevel - potentialDischargeBefore), 0);
            var chargingCapacity = _inverterLimit.Value - Charge;

            return Math.Min(chargingCapacity, Math.Min(currentRemaining, previousRemaining));
        }

        public int MaxDischarge(int potentialChargeBefore = 0)
        {
            if (potentialChargeBefore < 0 || potentialChargeBefore > _batterySize.Value - BatteryLevel)
                throw new ArgumentOutOfRangeException(nameof(potentialChargeBefore), "Must be between zero and remaining battery");

            var previousBatteryLevel = GetPreviousBatteryLevel();
            var currentRemaining = BatteryLevel + potentialChargeBefore;
            var previousRemaining = Math.Min(_batterySize.Value, previousBatteryLevel + potentialChargeBefore);
            var dischargingCapacity = _inverterLimit.Value + Charge;
            var possibleDischarge = Math.Max(Power * -1, 0);

            return Math.Min(dischargingCapacity, Math.Min(possibleDischarge, Math.Min(currentRemaining, previousRemaining)));
        }

        public int ChargeBattery(int value, int potentialDischargeBefore = 0)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");

            var charge = Math.Min(value, MaxCharge(potentialDischargeBefore));

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
            BatteryLevel += value;

            return charge;
        }

        public int DischargeBattery(int value, int potentialChargeBefore = 0)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");

            var discharge = Math.Min(value, MaxDischarge(potentialChargeBefore));

            Power += discharge;
            Charge -= discharge;
            BatteryLevel -= discharge;

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

        public BatteryMode BatteryMode()
        {
            return Model.BatteryMode.Default;
        }

        private int GetPreviousBatteryLevel()
        {
            return Math.Max(0, BatteryLevel - Charge);
        }
    }
}