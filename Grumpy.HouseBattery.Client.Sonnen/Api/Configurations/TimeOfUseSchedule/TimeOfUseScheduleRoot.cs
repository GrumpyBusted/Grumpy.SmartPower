using System.Text.Json.Serialization;

namespace Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.TimeOfUseSchedule;

internal class TimeOfUseScheduleRoot
{
    [JsonPropertyName("EM_ToU_Schedule")]
    public string TimeOfUseSchedule { get; set; } = "";
}