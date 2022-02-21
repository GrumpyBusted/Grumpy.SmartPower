using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Consumption;

public class PredictionWeatherData
{
    public WeatherItem Forecast { get; set; } = new();
    public WeatherItem LastWeek { get; set; } = new();
    public WeatherItem Yesterday { get; set; } = new();
    public WeatherItem LastWeekFromYesterday { get; set; } = new();
}