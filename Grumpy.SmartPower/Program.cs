using Grumpy.Common;
using Grumpy.Common.Interface;
using Grumpy.HouseBattery.Client.Sonnen;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.PowerMeter.Client.SmartMe;
using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.PowerPrice.Client.EnergyDataService;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.Rest;
using Grumpy.Rest.Interface;
using Grumpy.SmartPower;
using Grumpy.SmartPower.Core;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Interface;
using Grumpy.SmartPower.Core.Production;
using Grumpy.SmartPower.Infrastructure;
using Grumpy.SolarInformation;
using Grumpy.SolarInformation.Interface;
using Grumpy.Weather.Client.OpenWeatherMap;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Grumpy.Weather.Client.VisualCrossing;
using Grumpy.Weather.Client.VisualCrossing.Interface;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>()
            .AddSingleton<ISmartPowerService, SmartPowerService>()
            .AddSingleton<IHouseBatteryService, HouseBatteryService>()
            .AddSingleton<IPowerMeterService, PowerMeterService>()
            .AddSingleton<IPowerPriceService, PowerPriceService>()
            .AddSingleton<ISolarService, SolarService>()
            .AddSingleton<IWeatherService, WeatherService>()
            .AddSingleton<ISonnenBatteryClient, SonnenBatteryClient>()
            .AddSingleton<ISmartMePowerMeterClient, SmartMePowerMeterClient>()
            .AddSingleton<IEnergyDataServiceClient, EnergyDataServiceClient>()
            .AddSingleton<ISolarInformation, SolarInformation>()
            .AddSingleton<IOpenWeatherMapClient, OpenWeatherMapClient>()
            .AddSingleton<IRestClientFactory, RestClientFactory>()
            .AddSingleton<IProductionService, ProductionService>()
            .AddSingleton<IConsumptionService, ConsumptionService>()
            .AddSingleton<IPredictConsumptionService, PredictConsumptionService>()
            .AddSingleton<IVisualCrossingWeatherClient, VisualCrossingWeatherClient>()
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .Configure<WorkerOptions>(context.Configuration.GetSection("Application"))
            .Configure<SmartPowerServiceOptions>(context.Configuration.GetSection("SmartPower"))
            .Configure<SolarServiceOptions>(context.Configuration.GetSection("SmartPower:Location"))
            .Configure<SonnenBatteryClientOptions>(context.Configuration.GetSection("SonnenBattery"))
            .Configure<SmartMePowerMeterClientOptions>(context.Configuration.GetSection("SmartMePowerMeter"))
            .Configure<OpenWeatherMapClientOptions>(context.Configuration.GetSection("SmartPower:Location"))
            .Configure<OpenWeatherMapClientOptions>(context.Configuration.GetSection("OpenWeatherMap"))
            .Configure<ProductionServiceOptions>(context.Configuration.GetSection("SmartPower:SolarPanel"))
            .Configure<VisualCrossingWeatherClientOptions>(context.Configuration.GetSection("SmartPower:Location"))
            .Configure<VisualCrossingWeatherClientOptions>(context.Configuration.GetSection("VisualCrossingWeather"))
            .Configure<PredictConsumptionServiceOptions>(context.Configuration.GetSection("SmartPower:MachineLearning"));
    })
    .Build();

await host.RunAsync();
