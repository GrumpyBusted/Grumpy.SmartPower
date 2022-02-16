namespace Grumpy.SmartPower.Core.Production
{
    public interface IProductionService
    {
        IEnumerable<ProductionItem> Forecast(DateTime from, DateTime to);
    }
}