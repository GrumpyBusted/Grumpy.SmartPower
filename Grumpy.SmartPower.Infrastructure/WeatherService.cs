using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Grumpy.Weather.Client.VisualCrossing.Interface;

namespace Grumpy.SmartPower.Infrastructure
{
    public class WeatherService : IWeatherService
    {
        private readonly IOpenWeatherMapClient _openWeatherMapClient;
        private readonly IVisualCrossingWeatherClient _visualCrossingWeatherClient;

        public WeatherService(IOpenWeatherMapClient openWeatherMapClient, IVisualCrossingWeatherClient visualCrossingWeatherClient)
        {
            _openWeatherMapClient = openWeatherMapClient;
            _visualCrossingWeatherClient = visualCrossingWeatherClient;
        }

        public IEnumerable<WeatherItem> GetForecast(DateTime from, DateTime to)
        {
            return _openWeatherMapClient.GetForecast().Where(i => i.Hour >= from && i.Hour <= to).OrderBy(x => x.Hour);
        }

        public IEnumerable<WeatherItem> GetHistory(DateTime from, DateTime to)
        {
            return _visualCrossingWeatherClient.Get(from.ToDateOnly(), to.ToDateOnly()).Where(i => i.Hour >= from && i.Hour <= to).OrderBy(x => x.Hour);
        }

        public SunInformation GetSunInformation()
        {
            return _openWeatherMapClient.GetSunInformation();
        }
    }
}