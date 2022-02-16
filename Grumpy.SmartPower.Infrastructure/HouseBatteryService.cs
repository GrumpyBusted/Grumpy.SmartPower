using Grumpy.HouseBattery.Client.Sonnen.Dtos;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Infrastructure
{
    public class HouseBatteryService : IHouseBatteryService
    {
        private readonly ISonnenBatteryClient _sonnenBatteryClient;
        private readonly Lazy<int> _chargeFromGridWatt;

        public HouseBatteryService(ISonnenBatteryClient sonnenBatteryClient)
        {
            _sonnenBatteryClient = sonnenBatteryClient;
            _chargeFromGridWatt = new Lazy<int>(() => GetBatterySize());
        }

        public bool IsBatteryFull()
        {
            return _sonnenBatteryClient.GetBatteryLevel() > 99;
        }

        public int GetBatterySize()
        {
            var level = _sonnenBatteryClient.GetBatteryLevel();

            if (level == 0)
                return _sonnenBatteryClient.GetBatterySize();

            return (int)((double)_sonnenBatteryClient.GetBatteryCapacity() / level * 100);
        }

        public int GetBatteryCurrent()
        {
            return _sonnenBatteryClient.GetBatteryCapacity();
        }

        public void SetMode(BatteryMode batteryMode, DateTime hour)
        {
            var timeOfUseEvent = new TimeOfUseEvent()
            {
                Start = hour.ToString("HH:00"),
                End = hour.AddHours(1).ToString("HH:00")
            };

            switch (batteryMode)
            {
                case BatteryMode.StoreForLater:
                    timeOfUseEvent.Watt = 0;
                    break;
                case BatteryMode.ChargeFromGrid:
                    timeOfUseEvent.Watt = _chargeFromGridWatt.Value;
                    break;
                default:
                    timeOfUseEvent = null;
                    break;
            }

            if (timeOfUseEvent == null)
                _sonnenBatteryClient.SetOperatingMode(OperatingMode.SelfConsumption);
            else
            {
                _sonnenBatteryClient.SetSchedule(new List<TimeOfUseEvent>() { timeOfUseEvent });
                _sonnenBatteryClient.SetOperatingMode(OperatingMode.TimeOfUse);
            }
        }
    }
}