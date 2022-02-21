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
        private readonly IRealTimeReadingRepository _realTimeReadingRepository;

        public SmartPowerService(IOptions<SmartPowerServiceOptions> options, IPowerPriceService powerPriceService, IHouseBatteryService houseBatteryService, IProductionService productionService, IConsumptionService consumptionService, IRealTimeReadingRepository realTimeReadingRepository)
        {
            _options = options.Value;
            _powerPriceService = powerPriceService;
            _houseBatteryService = houseBatteryService;
            _productionService = productionService;
            _consumptionService = consumptionService;
            _realTimeReadingRepository = realTimeReadingRepository;
        }

        public void Execute(DateTime now)
        {
            var from = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var to = from.AddDays(1).AddSeconds(-1);

            var production = _productionService.Forecast(from, to).SerializeToJson();
            var consumption = _consumptionService.Predict(from, to).SerializeToJson();
            var prices = _powerPriceService.GetPrices(_options.PriceArea, _options.FallBackPriceArea, from, to).SerializeToJson();

            _houseBatteryService.GetBatterySize();
        }

        public void SaveData(DateTime now)
        {
            var production = _houseBatteryService.GetProduction();
            var consumption = _houseBatteryService.GetConsumption();

            _realTimeReadingRepository.Save(now, consumption, production);
        }
    }
}