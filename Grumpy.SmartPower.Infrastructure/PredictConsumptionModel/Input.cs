using Microsoft.ML.Data;

namespace Grumpy.SmartPower.Infrastructure.PredictConsumptionModel;

public class Input
{
    [LoadColumn(0)]
    public string Weekday { get; set; } = "";
    [LoadColumn(1)]
    public int Hour { get; set; }
    [LoadColumn(2)]
    public int Month { get; set; }
    [LoadColumn(3)]
    public int WattPerHourYesterday { get; set; }
    [LoadColumn(4)]
    public int WattPerHourLastWeek { get; set; }
    [LoadColumn(5)]
    public int WattPerHourLastWeekFromYesterday { get; set; }
    [LoadColumn(6)]
    public double WattPerHourWeekFactor { get; set; }
    [LoadColumn(7)]
    public double TemperatureForecast { get; set; }
    [LoadColumn(8)]
    public double WindSpeedForecast { get; set; }
    [LoadColumn(9)]
    public double TemperatureYesterday { get; set; }
    [LoadColumn(10)]
    public double WindSpeedYesterday { get; set; }
    [LoadColumn(11)]
    public double TemperatureLastWeek { get; set; }
    [LoadColumn(12)]
    public double WindSpeedLastWeek { get; set; }
    [LoadColumn(13)]
    public double TemperatureLastWeekFromYesterday { get; set; }
    [LoadColumn(14)]
    public double WindSpeedLastWeekFromYesterday { get; set; }
    [LoadColumn(15)]
    public int WattPerHour { get; set; }
}