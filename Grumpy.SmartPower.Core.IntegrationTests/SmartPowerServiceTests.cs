using FluentAssertions;
using Grumpy.Common;
using Grumpy.HouseBattery.Client.Sonnen;
using Grumpy.PowerMeter.Client.SmartMe;
using Grumpy.PowerPrice.Client.EnergyDataService;
using Grumpy.Rest;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Interface;
using Grumpy.SmartPower.Core.Production;
using Grumpy.SmartPower.Infrastructure;
using Grumpy.Weather.Client.OpenWeatherMap;
using Grumpy.Weather.Client.VisualCrossing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.IO;
using Grumpy.Caching;
using Xunit;

namespace Grumpy.SmartPower.Core.IntegrationTests;

public class SmartPowerServiceTests
{
    private readonly SmartPowerServiceOptions _smartPowerServiceOptions = new();
    private readonly SolarServiceOptions _solarServiceOptions = new();
    private readonly OpenWeatherMapClientOptions _openWeatherMapClientOptions = new();
    private readonly VisualCrossingWeatherClientOptions _visualCrossingWeatherOptions = new();
    private readonly ProductionServiceOptions _productionServiceOptions = new();
    private readonly SonnenBatteryClientOptions _sonnenBatteryClientOptions = new();
    private readonly SmartMePowerMeterClientOptions _smartMePowerMeterClientOptions = new();
    private readonly PredictConsumptionServiceOptions _predictConsumptionServiceOptions = new();
    private readonly PredictProductionServiceOptions _predictProductionServiceOptions = new();
    private readonly RealTimeReadingRepositoryOptions _realTimeReadingRepositoryOptions = new();
    private readonly CacheOptions _cacheOptions = new();
    private readonly PowerUsageRepositoryOptions _powerUsageRepositoryOptions = new();

    [Fact]
    public void SaveDataShouldSaveToFile()
    {
        _productionServiceOptions.Direction = 112;
        var cut = CreateTestObject();
        _realTimeReadingRepositoryOptions.RepositoryPath = $"Repository-{Guid.NewGuid()}.csv";
        _sonnenBatteryClientOptions.Ip = "192.168.0.222";
        _sonnenBatteryClientOptions.ApiToken = "0ca39846-405a-4c8f-b48c-41f46fe17be1";

        cut.SaveData(DateTime.Now);

        File.Exists(_realTimeReadingRepositoryOptions.RepositoryPath).Should().BeTrue();
    }

    private ISmartPowerService CreateTestObject()
    {
        var dateTimeProvider = new DateTimeProvider();
        var cacheFactory = new CacheFactory(Options.Create(_cacheOptions)); 
        var solarInformation = new SolarInformation.SolarInformation();
        var solarService = new SolarService(Options.Create(_solarServiceOptions), solarInformation);
        var restClientFactory = new RestClientFactory(Substitute.For<ILoggerFactory>());
        var openWeatherMapClient = new OpenWeatherMapClient(Options.Create(_openWeatherMapClientOptions), restClientFactory);
        var visualCrossingWeatherClient = new VisualCrossingWeatherClient(Options.Create(_visualCrossingWeatherOptions), restClientFactory);
        var weatherService = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient, dateTimeProvider, cacheFactory);
        var predictProductionService = new PredictProductionService(Options.Create(_predictProductionServiceOptions));
        var productionService = new ProductionService(Options.Create(_productionServiceOptions), solarService, weatherService, predictProductionService);
        var energyDataServiceClient = new EnergyDataServiceClient(restClientFactory);
        var powerPriceService = new PowerPriceService(energyDataServiceClient, cacheFactory);
        var sonnenBatteryClient = new SonnenBatteryClient(Options.Create(_sonnenBatteryClientOptions), restClientFactory);
        var houseBatteryService = new HouseBatteryService(sonnenBatteryClient, cacheFactory);
        var smartMePowerMeterClient = new SmartMePowerMeterClient(Options.Create(_smartMePowerMeterClientOptions), restClientFactory);
        var powerMeterService = new PowerMeterService(smartMePowerMeterClient, cacheFactory);
        var predictConsumptionService = new PredictConsumptionService(Options.Create(_predictConsumptionServiceOptions));
        var realTimeReadingRepository = new RealTimeReadingRepository(Options.Create(_realTimeReadingRepositoryOptions), cacheFactory);
        var consumptionService = new ConsumptionService(powerMeterService, weatherService, predictConsumptionService, realTimeReadingRepository);
        var logger = Substitute.For<ILogger<SmartPowerService>>();
        var powerUsageRepository = new PowerUsageRepository(Options.Create(_powerUsageRepositoryOptions));

        return new SmartPowerService(Options.Create(_smartPowerServiceOptions), powerPriceService, houseBatteryService, productionService, consumptionService, realTimeReadingRepository, logger, predictConsumptionService, predictProductionService, weatherService, powerUsageRepository, powerMeterService);
    }
}