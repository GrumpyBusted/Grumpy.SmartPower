namespace Grumpy.SmartPower.Core.Consumption;

public class ConsumptionItem
{
    public DateTime Hour { get; set; } = DateTime.MinValue;
    public int WattPerHour { get; set; } = 0;
}