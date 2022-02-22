using Microsoft.ML.Data;

namespace Grumpy.SmartPower.Infrastructure.PredictProductionModel;

public class Input
{
    [LoadColumn(0)]
    public int Hour { get; set; }
    [LoadColumn(1)]
    public int Month { get; set; }
    [LoadColumn(2)]
    public TimeSpan Sunlight { get; set; }
    [LoadColumn(3)]
    public double SunAltitude { get; set; }
    [LoadColumn(4)]
    public double SunDirection { get; set; }
    [LoadColumn(5)]
    public double Temperature { get; set; }
    [LoadColumn(6)]
    public int CloudCover { get; set; }
    [LoadColumn(7)]
    public double WindSpeed { get; set; }
    [LoadColumn(8)]
    public int Calculated { get; set; }
    [LoadColumn(9)]
    public int WattPerHour { get; set; }
}