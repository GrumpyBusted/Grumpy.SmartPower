using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.VisualCrossing.Api.Timeline
{
    public class Hour
    {
        [JsonPropertyName("datetimeEpoch")]
        public int DateTime { get; set; } = 0;
        [JsonPropertyName("temp")]
        public double Temperature { get; set; } = 0;
        [JsonPropertyName("windspeed")]
        public double WindSpeed { get; set; } = 0;
        [JsonPropertyName("cloudcover")]
        public double CloudCover { get; set; } = 0;
    }


}