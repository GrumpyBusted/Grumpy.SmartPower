using Grumpy.Json;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Interface;
using Grumpy.SmartPower.Core.Production;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower.Core
{
    public class SmartPowerService : ISmartPowerService
    {
        private readonly SmartPowerServiceOptions _options;
        private readonly IPowerPriceService _powerPriceService;
        private readonly IHouseBatteryService _houseBatteryService;
        private readonly IProductionService _productionService;
        private readonly IConsumptionService _consumptionService;

        public SmartPowerService(IOptions<SmartPowerServiceOptions> options, IPowerPriceService powerPriceService, IHouseBatteryService houseBatteryService, IProductionService productionService, IConsumptionService consumptionService)
        {
            _options = options.Value;
            _powerPriceService = powerPriceService;
            _houseBatteryService = houseBatteryService;
            _productionService = productionService;
            _consumptionService = consumptionService;
        } 

        public void Execute(DateTime now)
        {
            var from = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var to = from.AddDays(1).AddSeconds(-1);

            var production = _productionService.Forecast(from, to).SerializeToJson();
            var consumption = _consumptionService.Predict(from, to).SerializeToJson();
            var prices = _powerPriceService.GetPrices(_options.PriceArea, from, to);
            
            _houseBatteryService.GetBatterySize();
        }
    }
}