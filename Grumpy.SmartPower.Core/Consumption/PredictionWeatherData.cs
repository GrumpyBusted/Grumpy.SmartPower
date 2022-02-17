using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Consumption
{
    public class PredictionWeatherData
    {
        public WeatherItem Forecast { get; set; } = new WeatherItem();
        public WeatherItem LastWeek { get; set; } = new WeatherItem();
        public WeatherItem Yesterday { get; set; } = new WeatherItem();
        public WeatherItem LastWeekFromYesterday { get; set; } = new WeatherItem();
    }
}