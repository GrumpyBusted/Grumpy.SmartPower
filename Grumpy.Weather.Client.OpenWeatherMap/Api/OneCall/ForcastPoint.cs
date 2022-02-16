using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.OpenWeatherMap.Api.OneCall
{
    public class ForecastPoint
    {
        [JsonPropertyName("dt")]
        public int DateTime { get; set; } = 0;
        [JsonPropertyName("temp")]
        public double Temperature { get; set; } = 0;
        [JsonPropertyName("clouds")]
        public int Clouds { get; set; } = 0;
        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; set; } = 0;
    }
}