using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.OpenWeatherMap.Api.Weather
{
    public class Root
    {
        [JsonPropertyName("sys")]
        public SystemInformation SystemInformation { get; set; } = new SystemInformation();
    }
}
