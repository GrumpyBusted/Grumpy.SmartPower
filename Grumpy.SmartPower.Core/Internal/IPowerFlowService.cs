using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlowService
    {
        public BatteryMode Optimize(IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices, DateTime from);
        void ChargeExtraPower(IPowerFlow powerFlow);
        void DistributeInitialBatteryPower(IPowerFlow powerFlow);
        void ChargeFromGrid(IPowerFlow powerFlow);
        void DistributeBatteryPower(IPowerFlow powerFlow);
        double Price(IPowerFlow powerFlow);
    }
}