namespace Grumpy.HouseBattery.Client.Sonnen.Dto;

public class TimeOfUseEvent
{
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
    public int Watt { get; set; } = 0;
}