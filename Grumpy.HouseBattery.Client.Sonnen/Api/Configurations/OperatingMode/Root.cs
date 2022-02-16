using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.OperatingMode
{
    public class Root
    {
        [JsonPropertyName("EM_OperatingMode")]
        public string OperatingMode { get; set; } = "";
    }
}