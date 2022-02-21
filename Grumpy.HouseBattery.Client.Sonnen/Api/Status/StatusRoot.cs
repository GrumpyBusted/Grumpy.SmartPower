using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Status;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
internal class StatusRoot
{
    [JsonPropertyName("Production_W")]
    public int Production { get; set; } = 0;
    [JsonPropertyName("Consumption_AVG")]
    public int Consumption { get; set; } = 0;
    [JsonPropertyName("RemainingCapacity_Wh")]
    public int RemainingCapacity { get; set; } = 0;
    [JsonPropertyName("USOC")]
    public int UserStateOfCharge { get; set; } = 0;
}