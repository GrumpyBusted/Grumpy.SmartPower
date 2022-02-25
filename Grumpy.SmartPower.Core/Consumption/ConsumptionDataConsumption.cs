namespace Grumpy.SmartPower.Core.Consumption;

public class ConsumptionDataConsumption
{
    public int? LastWeek { get; set; }
    public int? Yesterday { get; set; }
    public int? LastWeekFromYesterday { get; set; }
    public double WeekFactor { get; set; }
}