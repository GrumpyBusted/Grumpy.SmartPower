using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.Caching.Interface;

namespace Grumpy.SmartPower.Infrastructure;

public class PowerMeterService : IPowerMeterService
{
    private readonly ISmartMePowerMeterClient _smartMePowerMeterClient;
    private readonly ICache _fileCache;

    public PowerMeterService(ISmartMePowerMeterClient smartMePowerMeterClient, ICacheFactory cacheFactory)
    {
        _smartMePowerMeterClient = smartMePowerMeterClient;
        _fileCache = cacheFactory.FileCacheInstance(GetType().FullName ?? nameof(PowerMeterService));
    }

    public int GetWattPerHour(DateTime hour)
    {
        var from = new DateTime(hour.Year, hour.Month, hour.Day, hour.Hour, 0, 0, hour.Kind);
        var to = from.AddHours(1);

        return (int)Math.Round((GetReading(to) - GetReading(from)) * 1000, 0);
    }

    public double GetReading(DateTime dateTime)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:Reading:{dateTime}", TimeSpan.FromDays(365),
            () => _smartMePowerMeterClient.GetValue(dateTime));
    }
}