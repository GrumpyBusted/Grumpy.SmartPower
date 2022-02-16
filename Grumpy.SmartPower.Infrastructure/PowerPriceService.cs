using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.SmartPower.Core.Dtos;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Infrastructure
{
    public class PowerPriceService : IPowerPriceService
    {
        private readonly IEnergyDataServiceClient _energyDataServiceClient;

        public PowerPriceService(IEnergyDataServiceClient energyDataServiceClient)
        {
            _energyDataServiceClient = energyDataServiceClient;
        }

        public IEnumerable<PriceItem> GetPrices(PriceArea priceArea, DateTime from, DateTime to)
        {
            var powerPrices = _energyDataServiceClient.GetPrices(priceArea, from, to).OrderBy(x => x.Hour);

            foreach (var i in powerPrices)
            {
                yield return new PriceItem()
                {
                    Hour = i.Hour,
                    Price = i.Price                
                };
            }
        }
    }
}