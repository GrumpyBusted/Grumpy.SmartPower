using Grumpy.Json;
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
        private readonly ISolarService _solarService;
        private readonly IPowerMeterService _powerMeterService;
        private readonly IProductionService _productionService;

        public SmartPowerService(IOptions<SmartPowerServiceOptions> options, IPowerPriceService powerPriceService, IHouseBatteryService houseBatteryService, ISolarService solarService, IPowerMeterService powerMeterService, IProductionService productionService)
        {
            _options = options.Value;
            _powerPriceService = powerPriceService;
            _houseBatteryService = houseBatteryService;
            _solarService = solarService;
            _powerMeterService = powerMeterService;
            _productionService = productionService;
        }

        public void Execute(DateTime now)
        {
            var from = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var to = from.AddDays(1).AddSeconds(-1);

            _productionService.Forecast(from, to);
            _powerPriceService.GetPrices(_options.PriceArea, now, now.AddDays(1));
            _houseBatteryService.GetBatterySize();
            _solarService.Sunlight(now);
            _powerMeterService.GetReading(now);
        }
    }
}