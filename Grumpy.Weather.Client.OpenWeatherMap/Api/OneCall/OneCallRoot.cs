using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.OpenWeatherMap.Api.OneCall
{
    internal class OneCallRoot
    {
        [JsonPropertyName("hourly")]
        public List<ForecastPoint> Forecast { get; set; } = new();
    }
}