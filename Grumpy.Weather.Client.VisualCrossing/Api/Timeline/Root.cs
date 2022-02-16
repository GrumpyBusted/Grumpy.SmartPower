using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.VisualCrossing.Api.Timeline
{
    internal class Root
    {
        [JsonPropertyName("days")]
        public List<Day> Days { get; set; } = new List<Day>();
    }
}