using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Caching.Interface;
using Grumpy.HouseBattery.Client.Sonnen.Dto;

namespace Grumpy.SmartPower.Infrastructure;

public class HouseBatteryService : IHouseBatteryService
{
    private readonly ISonnenBatteryClient _sonnenBatteryClient;
    private readonly Lazy<int> _chargeFromGridWatt;
    private readonly ICache _memoryCache;

    public HouseBatteryService(ISonnenBatteryClient sonnenBatteryClient, ICacheFactory cacheFactory)
    {
        _sonnenBatteryClient = sonnenBatteryClient;
        _chargeFromGridWatt = new Lazy<int>(GetBatterySize);
        _memoryCache = cacheFactory.MemoryCacheInstance(GetType().FullName ?? nameof(HouseBatteryService));
    }

    public BatteryMode GetBatteryMode()
    {
        return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:BatteryMode", TimeSpan.FromMinutes(1), GetBatteryModeInt);
    }

    public int InverterLimit()
    {
        return _sonnenBatteryClient.InverterLimit();
    }

    private BatteryMode GetBatteryModeInt()
    {
        var operatingMode = _sonnenBatteryClient.GetOperatingMode();

        if (operatingMode == OperatingMode.SelfConsumption)
            return BatteryMode.Default;

        var schedule = _sonnenBatteryClient.GetSchedule().ToList();

        if (schedule.Count != 1) 
            return BatteryMode.Manual;

        return schedule.FirstOrDefault()?.Watt == 0 ? BatteryMode.StoreForLater : BatteryMode.ChargeFromGrid;
    }

    public bool IsBatteryFull()
    {
        return GetBatteryLevel() > 99;
    }

    public int GetBatterySize()
    {
        return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:BatterySize", TimeSpan.FromHours(1), GetBatterySizeInt);
    }

    private int GetBatterySizeInt()
    {
        var level = GetBatteryLevel();

        if (level == 0)
            return _sonnenBatteryClient.GetBatterySize();

        return (int)((double)_sonnenBatteryClient.GetBatteryCapacity() / level * 100);
    }

    public int GetBatteryCurrent()
    {
        return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:BatteryCapacity", TimeSpan.FromMinutes(1),
            () => _sonnenBatteryClient.GetBatteryCapacity());
    }

    public void SetMode(BatteryMode batteryMode, DateTime hour)
    {
        var timeOfUseEvent = new TimeOfUseEvent
        {
            Start = hour.ToString("HH:00"),
            End = hour.AddHours(2).ToString("HH:00")
        };

        switch (batteryMode)
        {
            case BatteryMode.StoreForLater:
                timeOfUseEvent.Watt = 0;
                break;
            case BatteryMode.ChargeFromGrid:
                timeOfUseEvent.Watt = _chargeFromGridWatt.Value;
                break;
            case BatteryMode.Default:
            case BatteryMode.Manual:
            default:
                timeOfUseEvent = null;
                break;
        }

        if (timeOfUseEvent == null)
            _sonnenBatteryClient.SetOperatingMode(OperatingMode.SelfConsumption);
        else
        {
            _sonnenBatteryClient.SetSchedule(new List<TimeOfUseEvent> { timeOfUseEvent });
            _sonnenBatteryClient.SetOperatingMode(OperatingMode.TimeOfUse);
        }
    }

    public int GetBatteryLevel()
    {
        return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:BatteryLevel", TimeSpan.FromMinutes(1),
            () => _sonnenBatteryClient.GetBatteryLevel());
    }

    public int GetProduction()
    {
        return _sonnenBatteryClient.GetProduction();
    }

    public int GetConsumption()
    {
        return _sonnenBatteryClient.GetConsumption();
    }

    public int GetGridFeedIn()
    {
        return _sonnenBatteryClient.GetGridFeedIn();
    }
}