using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Core
{
    public class PowerFlowFactory : IPowerFlowFactory
    {
        private readonly IHouseBatteryService _houseBatteryService;

        public PowerFlowFactory(IHouseBatteryService houseBatteryService)
        {
            _houseBatteryService = houseBatteryService;
        }

        public IPowerFlow Instance()
        {
            return new PowerFlow(_houseBatteryService);
        }

        public IPowerFlow Instance(DateTime from, IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices)
        {
            return new PowerFlow(_houseBatteryService)
            {
                { from, productions, consumptions, prices }
            };
        }
    }
}
