using Grumpy.SmartPower.Core.Dtos;
using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Infrastructure
{
    public interface IPowerPriceService
    {
        public IEnumerable<PriceItem> GetPrices(PriceArea PriceArea, DateTime from, DateTime to);
    }
}