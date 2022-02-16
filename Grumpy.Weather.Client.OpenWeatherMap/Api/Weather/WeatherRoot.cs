using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.OpenWeatherMap.Api.Weather
{
    internal class WeatherRoot
    {
        [JsonPropertyName("sys")]
        public SystemInformation SystemInformation { get; set; } = new SystemInformation();
    }
}
