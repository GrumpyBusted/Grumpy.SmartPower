using Grumpy.SmartPower.Core.Dto;
using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IPowerPriceService
{
    public IEnumerable<PriceItem> GetPrices(PriceArea priceArea, PriceArea fallBackPriceArea, DateTime from, DateTime to);
}