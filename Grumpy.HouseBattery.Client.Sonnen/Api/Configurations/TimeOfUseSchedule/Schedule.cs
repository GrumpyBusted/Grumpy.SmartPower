using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.TimeOfUseSchedule
{
    public class Schedule
    {
        [JsonPropertyName("start")]
        public string Start { get; set; } = "";
        [JsonPropertyName("stop")]
        public string Stop { get; set; } = "";
        [JsonPropertyName("threshold_p_max")]
        public int Max { get; set; } = 0;
    }
}