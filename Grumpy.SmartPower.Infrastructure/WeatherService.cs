using Grumpy.Caching.Extensions;
using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using System.Runtime.Caching;

namespace Grumpy.SmartPower.Infrastructure
{
    public class WeatherService : IWeatherService
    {
        private readonly IOpenWeatherMapClient _openWeatherMapClient;
        private readonly IVisualCrossingWeatherClient _visualCrossingWeatherClient;
        private readonly FileCache _fileCache;
        private readonly MemoryCache _memoryCache;

        public WeatherService(IOpenWeatherMapClient openWeatherMapClient, IVisualCrossingWeatherClient visualCrossingWeatherClient)
        {
            _openWeatherMapClient = openWeatherMapClient;
            _visualCrossingWeatherClient = visualCrossingWeatherClient;
            _fileCache = new FileCache(FileCacheManagers.Hashed);
            _memoryCache = new MemoryCache(GetType().FullName);
        }

        public IEnumerable<WeatherItem> GetForecast(DateTime from, DateTime to)
        {
            return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:Forecast:{from}:{to}", TimeSpan.FromHours(1),
                () => _openWeatherMapClient.GetForecast().Where(i => i.Hour >= from && i.Hour <= to).OrderBy(x => x.Hour).ToList());
        }

        public IEnumerable<WeatherItem> GetHistory(DateTime from, DateTime to)
        {
            return _fileCache.TryGetIfNotSet($"{GetType().FullName}:History:{from}:{to}", TimeSpan.FromDays(365), 
                () => _visualCrossingWeatherClient.Get(from.ToDateOnly(), to.ToDateOnly()).Where(i => i.Hour >= from && i.Hour <= to).OrderBy(x => x.Hour).ToList());
        }

        public SunInformation GetSunInformation()
        {
            return _openWeatherMapClient.GetSunInformation();
        }
    }
}