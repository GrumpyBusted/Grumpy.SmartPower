using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.VisualCrossing.Api.Timeline;

internal class Day
{
    [JsonPropertyName("hours")]
    public List<Hour> Hours { get; set; } = new();
}