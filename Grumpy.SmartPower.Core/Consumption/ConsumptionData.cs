namespace Grumpy.SmartPower.Core.Consumption;

public class ConsumptionData
{
    public DateTime Hour { get; set; }
    public ConsumptionDataWeather Weather { get; set; } = new();
    public ConsumptionDataConsumption ConsumptionDataConsumption { get; set; } = new();
}