using Grumpy.SmartPower.Core.Dto;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Interface
{
    public interface IEnergyDataServiceClient
    {
        IEnumerable<Dto.PowerPrice> GetPrices(PriceArea priceArea, DateTime from, DateTime to);
        double GetExchangeRate(PriceArea priceArea, DateTime dateTime);
    }
}