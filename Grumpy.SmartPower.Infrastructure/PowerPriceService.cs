using Grumpy.Caching.Extensions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.SmartPower.Core.Dtos;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using System.Runtime.Caching;

namespace Grumpy.SmartPower.Infrastructure
{
    public class PowerPriceService : IPowerPriceService
    {
        private readonly IEnergyDataServiceClient _energyDataServiceClient;
        private readonly MemoryCache _memoryCache;

        public PowerPriceService(IEnergyDataServiceClient energyDataServiceClient)
        {
            _energyDataServiceClient = energyDataServiceClient;
            _memoryCache = new MemoryCache(GetType().FullName);
        }

        public IEnumerable<PriceItem> GetPrices(PriceArea priceArea, DateTime from, DateTime to)
        {
            return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:Prices:{priceArea}:{from}:{to}", TimeSpan.FromHours(1),
                () => GetPricesInt(priceArea, from, to));
        }

        private IEnumerable<PriceItem> GetPricesInt(PriceArea priceArea, DateTime from, DateTime to)
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