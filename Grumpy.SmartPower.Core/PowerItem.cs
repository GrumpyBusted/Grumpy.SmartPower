using Grumpy.SmartPower.Core.Infrastructure;

namespace Grumpy.SmartPower.Core
{
    public class PowerItem
    {
        private readonly IHouseBatteryService _houseBatteryService;

        public DateTime Hour { get; set; }
        public int Production { get; set; }
        public int Consumption { get; set; }
        public double Price { get; set; }
        public int BatteryLevel { get; set; }
        public int Power { get; set; }
        public int Charge { get; set; }

        public PowerItem(IHouseBatteryService houseBatteryService)
        {
            _houseBatteryService = houseBatteryService;
        }

        public int MaxCharge()
        {
            return MaxCharge(BatteryLevel);
        }

        public int MaxCharge(int maxBatteryLevel)
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var batterySize = _houseBatteryService.GetBatterySize();
            var remainingCapacity = batterySize - maxBatteryLevel;
            var chargingCapacity = inverterLimit - Charge;

            return Math.Min(chargingCapacity, remainingCapacity);
        }

        public int MaxDischarge()
        {
            return MaxDischarge(BatteryLevel);
        }

        public int MaxDischarge(int minBatteryLevel)
        {
            var inverterLimit = _houseBatteryService.InverterLimit();
            var dischargingCapacity = inverterLimit + Charge;
            var previousBatteryLevel = 
            var maxDischarge = Math.Min(dischargingCapacity, minBatteryLevel);
            var possibleDischarge = Math.Max(Power * -1, 0);

            return Math.Min(maxDischarge, possibleDischarge);
        }
    }
}