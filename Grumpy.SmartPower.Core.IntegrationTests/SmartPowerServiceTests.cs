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
    private readonly RealTimeReadingRepositoryOptions _realTimeReadingRepositoryOptions = new();

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
        var solarInformation = new SolarInformation.SolarInformation();
        var solarService = new SolarService(Options.Create(_solarServiceOptions), solarInformation);
        var restClientFactory = new RestClientFactory(Substitute.For<ILoggerFactory>());
        var openWeatherMapClient = new OpenWeatherMapClient(Options.Create(_openWeatherMapClientOptions), restClientFactory);
        var visualCrossingWeatherClient = new VisualCrossingWeatherClient(Options.Create(_visualCrossingWeatherOptions), restClientFactory);
        var weatherService = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient, dateTimeProvider);
        var productionService = new ProductionService(Options.Create(_productionServiceOptions), solarService, weatherService);
        var energyDataServiceClient = new EnergyDataServiceClient(restClientFactory);
        var powerPriceService = new PowerPriceService(energyDataServiceClient);
        var sonnenBatteryClient = new SonnenBatteryClient(Options.Create(_sonnenBatteryClientOptions), restClientFactory);
        var houseBatteryService = new HouseBatteryService(sonnenBatteryClient);
        var smartMePowerMeterClient = new SmartMePowerMeterClient(Options.Create(_smartMePowerMeterClientOptions), restClientFactory);
        var powerMeterService = new PowerMeterService(smartMePowerMeterClient);
        var predictConsumptionService = new PredictConsumptionService(Options.Create(_predictConsumptionServiceOptions));
        var realTimeReadingRepository = new RealTimeReadingRepository(Options.Create(_realTimeReadingRepositoryOptions));
        var consumptionService = new ConsumptionService(powerMeterService, weatherService, predictConsumptionService, realTimeReadingRepository);

        return new SmartPowerService(Options.Create(_smartPowerServiceOptions), powerPriceService, houseBatteryService, productionService, consumptionService, realTimeReadingRepository, Substitute.For<ILogger<SmartPowerService>>());
    }
}