using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.TimeOfUseSchedule
{
    public class Root
    {
        [JsonPropertyName("EM_ToU_Schedule")]
        public string TimeOfUseSchedule { get; set; } = "";
    }
}