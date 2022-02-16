using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.SmartPower.Core.Infrastructure;

namespace Grumpy.SmartPower.Infrastructure
{
    public class PowerMeterService : IPowerMeterService
    {
        private readonly ISmartMePowerMeterClient _smartMePowerMeterClient;

        public PowerMeterService(ISmartMePowerMeterClient smartMePowerMeterClient)
        {
            _smartMePowerMeterClient = smartMePowerMeterClient;
        }

        public double GetReading(DateTime dateTime)
        {
            return _smartMePowerMeterClient.GetValue(dateTime);
        }
    }
}