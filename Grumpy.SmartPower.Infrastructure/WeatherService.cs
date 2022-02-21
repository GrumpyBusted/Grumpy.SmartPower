using System.Diagnostics.CodeAnalysis;
using Grumpy.Caching.Extensions;
using Grumpy.Common.Extensions;
using Grumpy.Common.Interface;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using System.Runtime.Caching;

namespace Grumpy.SmartPower.Infrastructure;

public class WeatherService : IWeatherService
{
    private readonly IOpenWeatherMapClient _openWeatherMapClient;
    private readonly IVisualCrossingWeatherClient _visualCrossingWeatherClient;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly FileCache _fileCache;
    private readonly MemoryCache _memoryCache;

    public WeatherService(IOpenWeatherMapClient openWeatherMapClient, IVisualCrossingWeatherClient visualCrossingWeatherClient, IDateTimeProvider dateTimeProvider)
    {
        _openWeatherMapClient = openWeatherMapClient;
        _visualCrossingWeatherClient = visualCrossingWeatherClient;
        _dateTimeProvider = dateTimeProvider;
        _fileCache = new FileCache(FileCacheManagers.Hashed);
        _memoryCache = new MemoryCache(GetType().FullName ?? nameof(WeatherService));
    }

    public IEnumerable<WeatherItem> GetForecast(DateTime from, DateTime to)
    {
        var now = _dateTimeProvider.Now;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), "Invalid order of 'from' and 'to'");
        if (from < now.Date.AddHours(now.Hour))
            throw new ArgumentOutOfRangeException(nameof(from), "Most be in the future");

        return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:Forecast:{from}:{to}", TimeSpan.FromHours(1),
            () => _openWeatherMapClient.GetForecast().Where(i => i.Hour >= from && i.Hour <= to).OrderBy(x => x.Hour).ToList());
    }

    [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
    public IEnumerable<WeatherItem> GetHistory(DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), "Invalid order of 'from' and 'to'");
        if (to > _dateTimeProvider.Now)
            throw new ArgumentOutOfRangeException(nameof(to), "Most be in the passed");

        var list = new List<WeatherItem>();

        var today = _dateTimeProvider.Now.ToDateOnly();

        for (var date = from.ToDateOnly(); date <= to.ToDateOnly(); date = date.AddDays(1))
        {
            var timeout = date == today ? TimeSpan.FromHours(1) : TimeSpan.FromDays(365);

            list.AddRange(_fileCache.TryGetIfNotSet($"{GetType().FullName}:History:{date}", timeout, () => GetHistory(date)));
        }

        return list.Where(i => i.Hour >= from && i.Hour <= to).OrderBy(x => x.Hour);
    }

    private List<WeatherItem> GetHistory(DateOnly date)
    {
        return _visualCrossingWeatherClient.Get(date).ToList();
    }

    public SunInformation GetSunInformation()
    {
        return _openWeatherMapClient.GetSunInformation();
    }
}