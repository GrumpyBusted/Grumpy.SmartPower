using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Status
{
    public class Root
    {
        [JsonPropertyName("Production_W")]
        public int Production { get; set; } = 0;
        [JsonPropertyName("Consumption_W")]
        public int Consumption { get; set; } = 0;
        [JsonPropertyName("RemainingCapacity_Wh")]
        public int RemainingCapacity { get; set; } = 0;
        [JsonPropertyName("USOC")]
        public int UserStateOfCharge { get; set; } = 0;
    }
}
