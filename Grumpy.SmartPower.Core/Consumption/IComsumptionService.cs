namespace Grumpy.SmartPower.Core.Consumption
{
    public interface IComsumptionService
    {
        IEnumerable<ConsumptionItem> Predict(DateTime from, DateTime to);
    }
}