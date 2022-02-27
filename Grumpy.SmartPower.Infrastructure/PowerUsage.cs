namespace Grumpy.SmartPower.Infrastructure;

internal class PowerUsage
{
    public DateTime Hour { get; set; }
    public int Consumption { get; set; }
    public int Production { get; set; }
    public int GridFeedIn { get; set; }
    public int GridFeedOut { get; set; }
    public int MeterReading { get; set; }
    public int BatteryLevel { get; set; }
    public int BatteryCurrent { get; set; }
    public double Price { get; set; }
}