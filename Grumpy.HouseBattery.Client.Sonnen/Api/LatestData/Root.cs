using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.LatestData
{
    public class Root
    {
        [JsonPropertyName("FullChargeCapacity")]
        public int FullChargeCapacity { get; set; } = 0;
    }
}
