using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Production;

public class ProductionData
{
    public DateTime Hour { get; set; }
    public ProductionDataSun Sun { get; set; } = new();
    public WeatherItem Weather { get; set; } = new();
    public int Calculated { get; set; }
}