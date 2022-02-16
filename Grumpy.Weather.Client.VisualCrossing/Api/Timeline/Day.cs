using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.VisualCrossing.Api.Timeline
{
    public class Day
    {
        [JsonPropertyName("hours")]
        public List<Hour> Hours { get; set; } = new List<Hour>();
    }
}