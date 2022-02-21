namespace Grumpy.SmartPower.Core.Consumption;

public class PredictionConsumptionData
{
    public int LastWeek { get; set; }
    public int Yesterday { get; set; }
    public int LastWeekFromYesterday { get; set; }
}