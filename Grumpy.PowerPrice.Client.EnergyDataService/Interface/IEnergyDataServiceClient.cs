using Grumpy.SmartPower.Core.Dtos;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Interface
{
    public interface IEnergyDataServiceClient
    {
        IEnumerable<Dtos.PowerPrice> GetPrices(PriceArea priceArea, DateTime from, DateTime to);
    }
}