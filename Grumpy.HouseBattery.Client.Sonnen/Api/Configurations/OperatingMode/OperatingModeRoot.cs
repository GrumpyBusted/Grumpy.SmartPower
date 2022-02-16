using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.OperatingMode
{
    internal class OperatingModeRoot
    {
        [JsonPropertyName("EM_OperatingMode")]
        public string OperatingMode { get; set; } = "";
    }
}