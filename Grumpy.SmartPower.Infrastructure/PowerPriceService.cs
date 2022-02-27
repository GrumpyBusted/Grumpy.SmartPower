using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Caching.Interface;
using Grumpy.SmartPower.Core.Dto;

namespace Grumpy.SmartPower.Infrastructure;

public class PowerPriceService : IPowerPriceService
{
    private readonly IEnergyDataServiceClient _energyDataServiceClient;
    private readonly ICache _memoryCache;

    public PowerPriceService(IEnergyDataServiceClient energyDataServiceClient, ICacheFactory cacheFactory)
    {
        _energyDataServiceClient = energyDataServiceClient;
        _memoryCache = cacheFactory.MemoryCacheInstance(GetType().FullName ?? nameof(PowerPriceService));
    }

    public IEnumerable<PriceItem> GetPrices(PriceArea priceArea, PriceArea fallBackPriceArea, DateTime from, DateTime to)
    {
        return _memoryCache.TryGetIfNotSet($"{GetType().FullName}:Prices:{priceArea}:{fallBackPriceArea}:{from}:{to}", TimeSpan.FromHours(1),
            () => GetPricesInt(priceArea, fallBackPriceArea, from, to));
    }

    private IEnumerable<PriceItem> GetPricesInt(PriceArea priceArea, PriceArea fallBackPriceArea, DateTime from, DateTime to)
    {
        var powerPrices = _energyDataServiceClient.GetPrices(priceArea, from, to).OrderBy(x => x.Hour).ToList();

        Lazy<IEnumerable<PowerPrice.Client.EnergyDataService.Dto.PowerPrice>>? fallBackPrices = null;

        if (fallBackPriceArea != priceArea)
            fallBackPrices = new Lazy<IEnumerable<PowerPrice.Client.EnergyDataService.Dto.PowerPrice>>(() => _energyDataServiceClient.GetPrices(fallBackPriceArea, from, to));

        var list = new List<PriceItem>();

        foreach (var i in powerPrices)
        {
            AddToList(ref list, i, priceArea, i.Hour);
        }

        for (var hour = from > from.Date.AddHours(from.Hour) ? from.Date.AddHours(from.Hour + 1) : from; hour <= to; hour = hour.Add(TimeSpan.FromHours(1)))
        {
            if (list.Any(i => i.Hour == hour)) 
                continue;
                
            PowerPrice.Client.EnergyDataService.Dto.PowerPrice? item = null;

            if (fallBackPrices != null)
                item = fallBackPrices.Value.FirstOrDefault(p => p.Hour == hour);

            item ??= powerPrices.Where(i => i.Hour < hour).OrderByDescending(h => h.Hour).FirstOrDefault();

            if (fallBackPrices != null && item == null)
                item = fallBackPrices.Value.Where(p => p.Hour < hour).OrderByDescending(h => h.Hour)
                    .FirstOrDefault();

            if (fallBackPrices != null && item == null)
                item = fallBackPrices.Value.Where(p => p.Hour > hour).OrderBy(h => h.Hour).FirstOrDefault();

            if (item == null)
                continue;

            item.Hour = hour;
            AddToList(ref list, item, fallBackPriceArea, hour);
        }

        return list;
    }

    private void AddToList(ref List<PriceItem> list, PowerPrice.Client.EnergyDataService.Dto.PowerPrice item, PriceArea priceArea, DateTime hour)
    {
        var price = item.SpotPriceDKK ?? Math.Round(item.SpotPriceEUR * _energyDataServiceClient.GetExchangeRate(priceArea, hour) / 100, 2);

        list.Add(new PriceItem
        {
            Hour = hour,
            Price = Math.Round(price / 1000, 4)
        });
    }
}