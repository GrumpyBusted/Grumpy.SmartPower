namespace Grumpy.HouseBattery.Client.Sonnen.Dtos
{
    public class TimeOfUseEvent
    {
        public string Start { get; set; } = "";
        public string End { get; set; } = "";
        public int Watt { get; set; } = 0;
    }
}