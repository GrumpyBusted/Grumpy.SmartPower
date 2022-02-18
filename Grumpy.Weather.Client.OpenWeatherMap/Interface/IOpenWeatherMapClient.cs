using Grumpy.SmartPower.Core.Model;

namespace Grumpy.Weather.Client.OpenWeatherMap.Interface;

public interface IOpenWeatherMapClient
{
    public SunInformation GetSunInformation();
    public IEnumerable<WeatherItem> GetForecast();
}