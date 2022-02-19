using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.LatestData
{
    internal class LatestDataRoot
    {
        [JsonPropertyName("FullChargeCapacity")]
        public int FullChargeCapacity { get; set; } = 0;
    }
}