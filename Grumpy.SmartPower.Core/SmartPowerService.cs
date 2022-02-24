using System.Security.Principal;
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

            mode = GetBatteryMode(powerFlow);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Exception optimizing battery setting {exception}", exception);
        }
        finally
        {
            if (currentMode != mode)
            {
                // TODO: Remove
                File.AppendAllText("bin\\Trace.csv", $"{DateTime.Now:O};{mode}" + Environment.NewLine);

                _houseBatteryService.SetMode(mode, from);
            }
        }
    }

    private BatteryMode GetBatteryMode(IEnumerable<Item> powerFlow)
    {
        var inverterLimit = _houseBatteryService.InverterLimit();
        var batterySize = _houseBatteryService.GetBatterySize();

        var flow = powerFlow.OrderBy(i => i.Hour).ToList();

        var current = flow.FirstOrDefault();

        if (current == null)
            return BatteryMode.Default;

        foreach (var target in flow.Where(i => i.MissingPower > 0).OrderBy(i => i.Hour))
        {
            var startBatteryLevel = flow.Min(i => i.StartBatteryLevel);

            if (startBatteryLevel <= 0)
                break;

            var use = Math.Min(startBatteryLevel, target.MissingPower);
            target.MissingPower -= use;

            foreach (var item in flow.Where(i => i.Hour >= target.Hour))
                item.StartBatteryLevel -= use;
        }

        foreach (var target in flow.Where(i => i.MissingPower > 0).OrderByDescending(i => i.Price))
        {
            if (target.Hour == current.Hour || target.GridCharge > inverterLimit / 3)
                break;

            foreach (var source in flow.Where(i => i.Hour < target.Hour && i.ExtraPower > 0).OrderByDescending(i => i.Hour))
            {
                var move = Math.Min(target.MissingPower, source.ExtraPower);

                var level = flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour).Max(i => i.BatteryLevel + i.StartBatteryLevel);

                if (move + level > batterySize)
                    move = batterySize - level;

                target.MissingPower -= move;
                source.ExtraPower -= move;
                source.SolarCharge += move;

                foreach (var item in flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour))
                    item.BatteryLevel += move;

                if (target.MissingPower <= 0 || move + level >= batterySize)
                    break;
            }

            if (target.MissingPower <= 0)
                continue;

            foreach (var source in flow.Where(i => i.Hour < target.Hour && i.Price < target.Price && i.GridCharge + i.SolarCharge < inverterLimit).OrderBy(i => i.Price).ThenByDescending(i => i.Hour))
            {
                var charge = target.MissingPower > inverterLimit - source.GridCharge + source.GridCharge ? inverterLimit - source.GridCharge + source.SolarCharge : target.MissingPower;

                var level = flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour).Max(i => i.BatteryLevel + i.StartBatteryLevel);

                if (charge + level > batterySize)
                    charge = batterySize - level;

                target.MissingPower -= charge;
                source.GridCharge += charge;

                foreach (var item in flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour))
                    item.BatteryLevel += charge;

                if (target.MissingPower <= 0)
                    break;
            }
        }

        if (current.GridCharge > inverterLimit / 3)
            return BatteryMode.ChargeFromGrid;

        if (current.GridCharge > 0)
            return BatteryMode.StoreForLater;

        if (current.Consumption < current.Production)
            return BatteryMode.Default;

        return current.StartBatteryLevel + current.BatteryLevel > 0 ? BatteryMode.StoreForLater : BatteryMode.Default;
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
                Production = production.Value,
                Consumption = consumption.Value,
                MissingPower = consumption.Value > production.Value ? consumption.Value - production.Value : 0,
                ExtraPower = production.Value > consumption.Value ? production.Value - consumption.Value : 0,
                StartBatteryLevel = _houseBatteryService.GetBatteryCurrent(),
                Price = price.Value
            };
        }
    }

    internal class Item
    {
        public DateTime Hour { get; set; }
        public int Production { get; set; }
        public int Consumption { get; set; }
        public double Price { get; set; }
        public int BatteryLevel { get; set; }
        public int MissingPower { get; set; }
        public int ExtraPower { get; set; }
        public int GridCharge { get; set; }
        public int SolarCharge { get; internal set; }
        public int StartBatteryLevel { get; internal set; }
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

            if (weather == null)
                return;

            var consumption = _realTimeReadingRepository.GetConsumption(hour);

            if (consumption != null)
            {
                var consumptionData = _consumptionService.GetData(weather);

                _predictConsumptionService.FitModel(consumptionData, (int)consumption);
            }

            var production = _realTimeReadingRepository.GetProduction(hour);

            if (production == null)
                return;

            var productionData = _productionService.GetData(weather);

            _predictProductionService.FitModel(productionData, (int)production);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Updating machine learning models {exception}", ex);
        }
    }
}