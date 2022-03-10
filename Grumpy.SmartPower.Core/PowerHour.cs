using Grumpy.SmartPower.Core.Infrastructure;

namespace Grumpy.SmartPower.Core
{
    public class PowerHour
    {
        private readonly IHouseBatteryService _houseBatteryService;

        public DateTime Hour { get; set; }
        public int Production { get; set; }
        public int Consumption { get; set; }
        public double Price { get; set; }
        public int BatteryLevel { get; set; }
        public int Power { get; set; }
        public int Charge { get; set; }
        public int Grid { get; set; }

        public PowerHour(IHouseBatteryService houseBatteryService)
        {
            _houseBatteryService = houseBatteryService;
        }

        public int MaxCharge()
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var batterySize = _houseBatteryService.GetBatterySize();
            var previousBatteryLevel = GetPreviousBatteryLevel();
            var currentRemainingCapacity = batterySize - BatteryLevel - Charge;
            var previousRemainingCapacity = batterySize - previousBatteryLevel - Charge;
            var chargingCapacity = inverterLimit - Charge;

            return Math.Min(chargingCapacity, Math.Min(currentRemainingCapacity, previousRemainingCapacity));
        }

        public int MaxDischarge()
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var dischargingCapacity = inverterLimit + Charge;
            var previousBatteryLevel = GetPreviousBatteryLevel();
            var possibleDischarge = Math.Max(Power * -1, 0);

            return Math.Min(dischargingCapacity, Math.Min(possibleDischarge, previousBatteryLevel));
        }

        private int GetPreviousBatteryLevel()
        {
            return BatteryLevel - Charge;
        }
    }
}