using Grumpy.Json;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Interface;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower.Core;

public class SmartPowerService : ISmartPowerService
{
    private readonly SmartPowerServiceOptions _options;
    private readonly IPowerPriceService _powerPriceService;
    private readonly IHouseBatteryService _houseBatteryService;
    private readonly IProductionService _productionService;
    private readonly IConsumptionService _consumptionService;
    private readonly IRealTimeReadingRepository _realTimeReadingRepository;
    private readonly ILogger<SmartPowerService> _logger;
    private readonly IPredictConsumptionService _predictConsumptionService;
    private readonly IPredictProductionService _predictProductionService;
    private readonly IWeatherService _weatherService;

    public SmartPowerService(IOptions<SmartPowerServiceOptions> options, IPowerPriceService powerPriceService, IHouseBatteryService houseBatteryService, IProductionService productionService, IConsumptionService consumptionService, IRealTimeReadingRepository realTimeReadingRepository, ILogger<SmartPowerService> logger, IPredictConsumptionService predictConsumptionService, IPredictProductionService predictProductionService, IWeatherService weatherService)
    {
        _options = options.Value;
        _powerPriceService = powerPriceService;
        _houseBatteryService = houseBatteryService;
        _productionService = productionService;
        _consumptionService = consumptionService;
        _realTimeReadingRepository = realTimeReadingRepository;
        _logger = logger;
        _predictConsumptionService = predictConsumptionService;
        _predictProductionService = predictProductionService;
        _weatherService = weatherService;
    }

    public void Execute(DateTime now)
    {
        _logger.LogInformation("Optimize battery setting");

        var from = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var to = from.AddDays(1).AddSeconds(-1);
        var currentMode = BatteryMode.Default;
        var mode = BatteryMode.Default;

        try
        {
            currentMode = _houseBatteryService.GetBatteryMode();

            var powerFlow = PowerFlow(from, to);

            mode = GetBatteryMode(powerFlow.ToList());
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Exception optimizing battery setting {exception}", exception);
        }
        finally
        {
            if (currentMode != mode)
                _houseBatteryService.SetMode(mode, from);
        }
    }

    private BatteryMode GetBatteryMode(List<Item> powerFlow)
    {
        var mode = BatteryMode.Default;

        //if (!batteryFull && priceLowerNow && NeedToBuyPower())
        //    mode = BatteryMode.ChargeFromGrid;


        //if ()
        //{
        //    if (false)
        //        mode = BatteryMode.StoreForLater;
        //}
        //else
        //{

        //}

        return mode;
    }

    private IEnumerable<Item> PowerFlow(DateTime from, DateTime to)
    {
        var productionList = _productionService.Predict(from, to).ToList();
        var consumptionList = _consumptionService.Predict(from, to).ToList();
        var priceList = _powerPriceService.GetPrices(_options.PriceArea, _options.FallBackPriceArea, from, to).ToList();

        for (var hour = from; hour < to; hour = hour.AddHours(1))
        {
            var production = productionList.FirstOrDefault(p => p.Hour == hour)?.WattPerHour;
            var consumption = consumptionList.FirstOrDefault(p => p.Hour == hour)?.WattPerHour;
            var price = priceList.FirstOrDefault(p => p.Hour == hour)?.Price;

            if (production == null || consumption == null || price == null)
                break;

            yield return new Item
            {
                Hour = hour,
                Production = production ?? 0,
                Consumption = consumption ?? 0,
                Price = price ?? 0
            };
        }
    }

    internal class Item
    {
        public DateTime Hour { get; set; }
        public int Production { get; set; }
        public int Consumption { get; set; }
        public double Price { get; set; }
    }

    public void SaveData(DateTime now)
    {
        _logger.LogInformation("Updating real time reading repository");

        try
        {
            var production = _houseBatteryService.GetProduction();
            var consumption = _houseBatteryService.GetConsumption();

            _realTimeReadingRepository.Save(now, consumption, production);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Exception updating real time reading repository {exception}", ex);
        }
    }

    public void UpdateModel(DateTime now)
    {
        var hour = now.Date.AddHours(now.Hour - 1);

        _logger.LogInformation("Updating machine learning models {hour}", hour);

        try
        {
            var weather = _weatherService.GetHistory(hour, hour.AddHours(1).AddMilliseconds(-1)).FirstOrDefault();

            if (weather != null)
            {
                var consumption = _realTimeReadingRepository.GetConsumption(hour);

                if (consumption != null)
                {
                    var consumptionData = _consumptionService.GetData(weather);

                    _predictConsumptionService.FitModel(consumptionData, (int)consumption);
                }

                var production = _realTimeReadingRepository.GetProduction(hour);

                if (production != null)
                {
                    var productionData = _productionService.GetData(weather);

                    _predictProductionService.FitModel(productionData, (int)production);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Updating machine learning models {exception}", ex);
        }
    }
}