using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Consumption;

public interface IConsumptionService
{
    IEnumerable<ConsumptionItem> Predict(DateTime from, DateTime to);
    ConsumptionData GetData(WeatherItem item);
}