using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Production;

public interface IProductionService
{
    IEnumerable<ProductionItem> Predict(DateTime from, DateTime to);
    ProductionData GetData(WeatherItem item);
}