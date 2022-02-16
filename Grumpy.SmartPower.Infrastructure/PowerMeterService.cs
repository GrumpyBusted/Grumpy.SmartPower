using Grumpy.Caching.Extensions;
using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.SmartPower.Core.Infrastructure;
using System.Runtime.Caching;

namespace Grumpy.SmartPower.Infrastructure
{
    public class PowerMeterService : IPowerMeterService
    {
        private readonly ISmartMePowerMeterClient _smartMePowerMeterClient;
        private readonly FileCache _fileCache;

        public PowerMeterService(ISmartMePowerMeterClient smartMePowerMeterClient)
        {
            _smartMePowerMeterClient = smartMePowerMeterClient;
            _fileCache = new FileCache(FileCacheManagers.Hashed);
        }

        public double GetReading(DateTime dateTime)
        {
            return _fileCache.TryGetIfNotSet($"{GetType().FullName}:Reading:{dateTime}", TimeSpan.FromDays(365),
                () => _smartMePowerMeterClient.GetValue(dateTime));
        }
    }
}