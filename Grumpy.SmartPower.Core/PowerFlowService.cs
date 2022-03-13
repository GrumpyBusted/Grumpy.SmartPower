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
            var powerFlow = _powerFlowFactory.Instance();

            powerFlow.Add(from, productions, consumptions, prices);

            ChargeExtraPower(powerFlow);

            return BatteryMode.Default;
        }

        public static void ChargeExtraPower(IPowerFlow powerFlow)
        {
            foreach (var source in powerFlow.Where(i => i.Power > 0).OrderBy(i => i.Price))
            {
                powerFlow.Charge(source);
            }
        }

        public static void DistributeExtraPower(IPowerFlow powerFlow)
        {
            foreach (var source in powerFlow.Where(i => i.Power > 0).OrderBy(i => i.Hour))
            {
                foreach (var target in powerFlow.Where(i => i.Hour < source.Hour && i.MaxDischarge() > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
                {
                    // TODO Move
                    var move = powerFlow.Discharge(target);

                    powerFlow.Charge(source, move);

                    if (source.Power <= 0)
                        break;
                }
            }
        }

        public void DistributeBatteryPower(IPowerFlow powerFlow)
        {
            foreach (var target in _flow.Where(i => MaxDischarge(i) > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                //target.Power += DischargeBattery(target, target.Power * -1);
            }
        }

        public void DistributeInitialBatteryPower(IPowerFlow powerFlow)
        {
            //var index = _flow.Where(i => MaxDischarge(i) > 0).OrderBy(i => i.Hour).FirstOrDefault();

            //while (index != null)
            //{
            //    var recharge = _flow.Where(i => i.Hour > index.Hour && i.Price < index.Price * _chargeEfficiency && MaxCharge(i) > i.OptionalRecharge).OrderBy(i => i.Hour).FirstOrDefault();

            //    if (recharge == null)
            //    {
            //        index = _flow.Where(i => MaxDischarge(i) > 0 && i.Hour > index.Hour).OrderBy(i => i.Hour).FirstOrDefault();

            //        continue;
            //    }

            //    var discharge = _flow.Where(i => i.Hour >= index.Hour && i.Hour < recharge.Hour && MaxDischarge(i) > 0).OrderByDescending(i => i.Price).FirstOrDefault();

            //    if (discharge != null)
            //    {
            //        var move = DischargeBattery(discharge, discharge.Power * -1);
            //        discharge.Power += move;
            //        recharge.OptionalRecharge += move;
            //    }

            //    index = _flow.Where(i => MaxDischarge(i) > 0).OrderBy(i => i.Hour).FirstOrDefault();
            //}
        }

        public double Price(IPowerFlow powerFlow)
        {
            //var res = (_flow.OrderBy(i => i.Hour).FirstOrDefault()?.Price ?? 0) * _houseBatteryService.GetBatteryCurrent() * -1;

            //foreach (var item in _flow.OrderBy(i => i.Hour))
            //{
            //    res += item.Price * item.Power + item.Charge - Math.Max(0, item.Production - item.Consumption);
            //}

            //res += _flow.OrderBy(i => i.Hour).LastOrDefault()?.Price ?? 0;

            return 1;
        }

        public void ChargeFromGrid(IPowerFlow powerFlow)
        {
            //foreach (var target in _flow.Where(i => i.Power < 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            //{
            //    foreach (var source in _flow.Where(i => i.Hour < target.Hour && i.Price < target.Price * _chargeEfficiency).OrderBy(i => i.Price).ThenByDescending(i => i.Hour))
            //    {
            //        //target.Power += MovePower(source, target, target.Power * -1);

            //        if (target.Power >= 0)
            //            break;
            //    }
            //}
        }

        public void ChargeExtraPower()
        {
            throw new NotImplementedException();
        }
    }
}
