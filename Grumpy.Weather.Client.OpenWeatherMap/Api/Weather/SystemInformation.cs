using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.OpenWeatherMap.Api.Weather;

internal class SystemInformation
{
    [JsonPropertyName("sunrise")]
    public int Sunrise { get; set; } = 0;
    [JsonPropertyName("sunset")]
    public int Sunset { get; set; } = 0;
}