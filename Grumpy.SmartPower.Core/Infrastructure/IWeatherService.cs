using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IWeatherService
{
    public SunInformation GetSunInformation();
    public IEnumerable<WeatherItem> GetForecast(DateTime from, DateTime to);
    public IEnumerable<WeatherItem> GetHistory(DateTime from, DateTime to);
}