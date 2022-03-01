using Grumpy.Json;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Interface;
using Grumpy.SmartPower.Core.Internal;
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
    private readonly IPowerUsageRepository _powerUsageRepository;
    private readonly IPowerMeterService _powerMeterService;
    private readonly IPowerFlowFactory _powerFlowFactory;

    public SmartPowerService(IOptions<SmartPowerServiceOptions> options, IPowerPriceService powerPriceService, IHouseBatteryService houseBatteryService, IProductionService productionService, IConsumptionService consumptionService, IRealTimeReadingRepository realTimeReadingRepository, ILogger<SmartPowerService> logger, IPredictConsumptionService predictConsumptionService, IPredictProductionService predictProductionService, IWeatherService weatherService, IPowerUsageRepository powerUsageRepository, IPowerMeterService powerMeterService, IPowerFlowFactory powerFlowFactory)
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
        _powerUsageRepository = powerUsageRepository;
        _powerMeterService = powerMeterService;
        _powerFlowFactory = powerFlowFactory;
    }

    public void Execute(DateTime now)
    {
        _logger.LogInformation("Optimize battery setting");

        var from = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var to = from.AddDays(1).AddSeconds(-1);
        var mode = BatteryMode.Default;

        try
        {
            mode = OptimizeBattery(from, to);

            //var powerFlow = GetPowerFlow(from, to);

            //mode = GetBatteryMode(powerFlow);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Exception optimizing battery setting {exception}", exception);
        }
        finally
        {
            _houseBatteryService.SetMode(mode, from);
        }
    }

    private BatteryMode OptimizeBattery(DateTime from, DateTime to)
    {
        var productions = _productionService.Predict(from, to);
        var consumptions = _consumptionService.Predict(from, to);
        var prices = _powerPriceService.GetPrices(_options.PriceArea, _options.FallBackPriceArea, from, to);

        var flow = _powerFlowFactory.Instance(productions, consumptions, prices, from, to);

        flow.DistributeExtraSolarPower();
        flow.DistributeInitialBatteryPower();
        flow.ChargeFromGrid();
        flow.DistributeBatteryPower();

        return BatteryMode.Default;
    }

    private BatteryMode GetBatteryMode(IEnumerable<Item> powerFlow)
    {
        var inverterLimit = _houseBatteryService.InverterLimit();
        var batterySize = _houseBatteryService.GetBatterySize();
        var startBatteryLevel = _houseBatteryService.GetBatteryCurrent();
        int chargeCapacity = Math.Min(batterySize, inverterLimit);

        var flow = powerFlow.OrderBy(i => i.Hour).ToList();

        var current = flow.FirstOrDefault();

        if (current == null)
            return BatteryMode.Default;

        foreach (var source in flow.Where(i => i.ExtraPower > 0).OrderBy(i => i.Hour))
        {
            source.ExtraPower -= ChargeBattery(flow, source, source.ExtraPower);

            if (source.ExtraPower <= 0)
                continue;

            foreach (var target in flow.Where(i => i.Hour < source.Hour && i.MissingPower > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
            {
                source.ExtraPower -= DischargeBattery(flow, target, source.ExtraPower);

                if (source.ExtraPower <= 0)
                    break;
            }
        }

        foreach (var target in flow.Where(i => i.MissingPower > 0).OrderByDescending(i => i.Price).ThenByDescending(i => i.Price).ThenBy(i => i.Hour))
        {
            if (target.BatteryLevel - target.SaveForLater > 0)
            {
                if (!flow.Any(i => i.Hour < target.Hour && i.Price < current.Price * _options.ChargeEfficient))
                {
                    var maxBatteryLevel = flow.Where(i => i.Hour >= target.Hour).Min(i => i.BatteryLevel);
                    var maxUse = Math.Min(inverterLimit, maxBatteryLevel);
                    var use = Math.Min(maxUse, target.MissingPower);

                    foreach (var hour in flow.Where(i => i.Hour >= target.Hour))
                        hour.BatteryLevel -= use;

                    foreach (var hour in flow.Where(i => i.Hour < target.Hour))
                        hour.SaveForLater += use;

                    target.MissingPower -= use;
                    target.Discharge += use;
                }
            }
        }

        foreach (var target in flow.Where(i => i.MissingPower > 0).OrderByDescending(i => i.Price).ThenBy(i => i.Hour))
        {
            foreach (var source in flow.Where(i => i.Hour < target.Hour && i.Price < target.Price * _options.ChargeEfficient && i.GridCharge + i.Charge < chargeCapacity && i.Discharge <= 0).OrderBy(i => i.Price).ThenByDescending(i => i.Hour))
            {
                int maxCharge = chargeCapacity - source.GridCharge - source.Charge;
                var maxBatteryLevel = flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour).Max(i => i.BatteryLevel);
                var maxChargeTo = (int)Math.Round(_options.ChargeFromGridLimit * batterySize);
                var charge = Math.Min(target.MissingPower, maxCharge);
                charge = Math.Min(charge, maxChargeTo - maxBatteryLevel);

                if (charge <= 0)
                    continue;

                target.MissingPower -= charge;
                target.Discharge += charge;
                source.GridCharge += charge;

                foreach (var item in flow.Where(i => i.Hour >= source.Hour && i.Hour < target.Hour))
                {
                    item.BatteryLevel += charge;
                    item.SaveForLater += charge;
                }

                if (target.MissingPower <= 0)
                    break;
            }
        }

        BatteryMode mode;

        if (current.GridCharge > 0)
            mode = BatteryMode.ChargeFromGrid;
        else if (current.Discharge > 0)
            mode = BatteryMode.Default;
        else
        {
            var batteryLevelAtBeginning = _houseBatteryService.GetBatteryCurrent();
            mode = current.BatteryLevel - batteryLevelAtBeginning > Math.Max(current.Production - current.Consumption, 0) || current.BatteryLevel > 0 && current.BatteryLevel == batteryLevelAtBeginning ? BatteryMode.StoreForLater : BatteryMode.Default;
        }

        // TODO: Remove
        if (!Directory.Exists(_options.TraceFilePath))
            Directory.CreateDirectory(_options.TraceFilePath);

        File.WriteAllText($"{_options.TraceFilePath}\\{DateTime.Now:yyyy-MM-dd HH-mm-ss}-{mode}.json", flow.SerializeToJson());

        return mode;
    }

    private int ChargeBattery(List<Item> flow, Item source, int charge)
    {
        var inverterLimit = _houseBatteryService.InverterLimit();
        var batterySize = _houseBatteryService.GetBatterySize();
        var batteryLevel = flow.Where(i => i.Hour >= source.Hour).Max(i => i.BatteryLevel);
        var remainingCapacity = batterySize - batteryLevel;
        int chargingCapacity = inverterLimit - source.Charge;
        var maxCharge = Math.Min(chargingCapacity, remainingCapacity);
        var res = Math.Min(maxCharge, charge);

        source.Charge += res;

        foreach (var hour in flow.Where(i => i.Hour >= source.Hour))
            hour.BatteryLevel += res;

        return res;
    }

    private int DischargeBattery(List<Item> flow, Item target, int use)
    {
        var inverterLimit = _houseBatteryService.InverterLimit();
        var batterySize = _houseBatteryService.GetBatterySize();
        int dischargingCapacity = inverterLimit - target.Discharge;
        var maxUse = Math.Min(dischargingCapacity, Math.Min(target.BatteryLevel, target.MissingPower));
        var res = Math.Min(maxUse, use);

        target.MissingPower -= res;
        target.Discharge += res;

        foreach (var hour in flow.Where(i => i.Hour >= target.Hour))
            hour.BatteryLevel -= Math.Min(hour.BatteryLevel, res);

        return res;
    }

    private IEnumerable<Item> GetPowerFlow(DateTime from, DateTime to)
    {
        var productionList = _productionService.Predict(from, to).ToList();
        var consumptionList = _consumptionService.Predict(from, to).ToList();
        var priceList = _powerPriceService.GetPrices(_options.PriceArea, _options.FallBackPriceArea, from, to).ToList();
        var batteryLevel = _houseBatteryService.GetBatteryCurrent();

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
                MissingPower = Math.Max(consumption.Value - production.Value, 0),
                ExtraPower = Math.Max(production.Value - consumption.Value, 0),
                BatteryLevel = batteryLevel,
                Price = price.Value
            };
        }
    }

    public void SaveData(DateTime now)
    {
        _logger.LogInformation("Updating real time reading repository");

        try
        {
            var production = _houseBatteryService.GetProduction();
            var consumption = _houseBatteryService.GetConsumption();
            var gridFeedIn = _houseBatteryService.GetGridFeedIn();

            _realTimeReadingRepository.Save(now, consumption, production, gridFeedIn);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Exception updating real time reading repository {exception}", ex);
        }
    }

    public void SavePowerUsage(DateTime now)
    {
        _logger.LogInformation("Updating power usage repository");

        try
        {
            var hour = now.Date.AddHours(now.Hour - 1);

            var production = _realTimeReadingRepository.GetProduction(hour) ?? -1;
            var consumption = _realTimeReadingRepository.GetConsumption(hour) ?? -1;
            var gridFeedIn = _realTimeReadingRepository.GetGridFeedIn(hour) ?? -1;
            var gridFeedOut = _realTimeReadingRepository.GetGridFeedOut(hour) ?? -1;
            var meterReading = _powerMeterService.GetWattPerHour(hour);
            var batteryLevel = _houseBatteryService.GetBatteryLevel();
            var batteryCurrent = _houseBatteryService.GetBatteryCurrent();
            var price = _powerPriceService.GetPrices(_options.PriceArea, _options.FallBackPriceArea, hour, hour.AddMinutes(59)).FirstOrDefault()?.Price ?? -1;

            _powerUsageRepository.Save(hour, consumption, production, gridFeedIn, gridFeedOut, meterReading, batteryLevel, batteryCurrent, price);
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