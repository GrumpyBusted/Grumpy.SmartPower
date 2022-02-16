using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.OpenWeatherMap.Api.OneCall
{
    public class Root
    {
        [JsonPropertyName("hourly")]
        public List<ForecastPoint> Forecast { get; set; } = new List<ForecastPoint>();
    }
}
