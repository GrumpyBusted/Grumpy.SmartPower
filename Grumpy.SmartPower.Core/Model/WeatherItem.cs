namespace Grumpy.SmartPower.Core.Model;

[Serializable]
public class WeatherItem
{
    public DateTime Hour { get; set; }
    public double Temperature { get; set; }
    public int CloudCover { get; set; }
    public double WindSpeed { get; set; }
}