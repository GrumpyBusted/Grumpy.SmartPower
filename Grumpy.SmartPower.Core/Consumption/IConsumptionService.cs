namespace Grumpy.SmartPower.Core.Consumption
{
    public interface IConsumptionService
    {
        IEnumerable<ConsumptionItem> Predict(DateTime from, DateTime to);
    }
}