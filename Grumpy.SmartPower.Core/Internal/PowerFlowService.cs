using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Core
{

    public class PowerFlowService : IPowerFlowService
    {
        private readonly IHouseBatteryService _houseBatteryService;
        private readonly IPowerFlowFactory _powerFlowFactory;
        private readonly double _chargeEfficiency = 0.80;

        public PowerFlowService(IHouseBatteryService houseBatteryService, IPowerFlowFactory powerFlowFactory)
        {
            _houseBatteryService = houseBatteryService;
            _powerFlowFactory = powerFlowFactory;
        }

        public BatteryMode Optimize(IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices, DateTime from)
        {
            var powerFlow = _powerFlowFactory.Instance(from, productions, consumptions, prices);

            ChargeExtraPower(powerFlow);

            return BatteryMode.Default;
        }

        public void ChargeExtraPower(IPowerFlow powerFlow)
        {
            foreach (var source in powerFlow.Where(i => i.Power > 0).OrderBy(i => i.Price))
            {
                powerFlow.Charge(source);
            }
        }

        public void DistributeExtraPower(IPowerFlow powerFlow)
        {
            foreach (var source in powerFlow.Where(i => i.Power > 0).OrderBy(i => i.Hour))
            {
                foreach (var target in powerFlow.Where(i => i.Hour < source.Hour && i.MaxDischarge() > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
                {
                    powerFlow.Move(source, target);

                    if (source.Power <= 0)
                        break;
                }
            }
        }

        public void DistributeBatteryPower(IPowerFlow powerFlow)
        {
            foreach (var target in powerFlow.Where(i => i.MaxDischarge() > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                powerFlow.Discharge(target);
            }
        }

        public void DistributeInitialBatteryPower(IPowerFlow powerFlow)
        {
            var index = powerFlow.Where(i => i.MaxDischarge() > 0).OrderBy(i => i.Hour).FirstOrDefault();

            while (index != null)
            {
                var recharge = powerFlow.Where(i => i.Hour > index.Hour && i.Price < index.Price * _chargeEfficiency/* && i.MaxCharge() > i.OptionalRecharge*/).OrderBy(i => i.Hour).FirstOrDefault();

                if (recharge == null)
                {
                    index = powerFlow.Where(i => i.MaxDischarge() > 0 && i.Hour > index.Hour).OrderBy(i => i.Hour).FirstOrDefault();

                    continue;
                }

                var discharge = powerFlow.Where(i => i.Hour >= index.Hour && i.Hour < recharge.Hour && i.MaxDischarge() > 0).OrderByDescending(i => i.Price).FirstOrDefault();

                if (discharge != null)
                {
                    var move = powerFlow.Discharge(discharge);
                    //recharge.OptionalRecharge += move;
                }

                index = powerFlow.Where(i => i.MaxDischarge() > 0).OrderBy(i => i.Hour).FirstOrDefault();
            }
        }

        public void ChargeFromGrid(IPowerFlow powerFlow)
        {
            foreach (var target in powerFlow.Where(i => i.Power < 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                foreach (var source in powerFlow.Where(i => i.Hour < target.Hour && i.Price < target.Price * _chargeEfficiency).OrderBy(i => i.Price).ThenByDescending(i => i.Hour))
                {
                    powerFlow.Move(source, target);

                    if (target.Power >= 0)
                        break;
                }
            }
        }

        public double Price(IPowerFlow powerFlow)
        {
            var firstPrice = powerFlow.First()?.Price ?? 0;
            var beginingBatteryLevel = powerFlow.First()?.BatteryLevel ?? 0 - powerFlow.First()?.Charge ?? 0;
            var res = firstPrice * beginingBatteryLevel;

            foreach (var item in powerFlow.OrderBy(i => i.Hour))
            {
                res += item.Price * (item.Power + item.Grid);
            }

            var lastPrice = powerFlow.Last()?.Price ?? 0;
            var endBatteryLevel = powerFlow.First()?.BatteryLevel ?? 0;
            res += lastPrice * endBatteryLevel * -1;

            return res;
        }
    }
}
