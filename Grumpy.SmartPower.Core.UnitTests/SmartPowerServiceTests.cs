using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Production;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Grumpy.SmartPower.Core.Dto;
using Xunit;
using Microsoft.Extensions.Logging;
using Grumpy.SmartPower.Core.Model;
using System.Collections.Generic;

namespace Grumpy.SmartPower.Core.UnitTests;

public class SmartPowerServiceTests
{
    private readonly SmartPowerServiceOptions _options = new()
    {
        PriceArea = PriceArea.DK2
    };

    private readonly IPowerPriceService _powerPriceService = Substitute.For<IPowerPriceService>();
    private readonly IHouseBatteryService _houseBatteryService = Substitute.For<IHouseBatteryService>();
    private readonly IProductionService _productionService = Substitute.For<IProductionService>();
    private readonly IConsumptionService _consumptionService = Substitute.For<IConsumptionService>();
    private readonly IRealTimeReadingRepository _realTimeReadingRepository = Substitute.For<IRealTimeReadingRepository>();
    private readonly ILogger<SmartPowerService> _logger = Substitute.For<ILogger<SmartPowerService>>();
    private readonly IPredictConsumptionService _predictConsumptionService = Substitute.For<IPredictConsumptionService>();
    private readonly IPredictProductionService _predictProductionService = Substitute.For<IPredictProductionService>();
    private readonly IWeatherService _weatherService = Substitute.For<IWeatherService>();
    private readonly IPowerUsageRepository _powerUsageRepository = Substitute.For<IPowerUsageRepository>();
    private readonly IPowerMeterService _powerMeterService = Substitute.For<IPowerMeterService>();
    private readonly IPowerFlowFactory _powerFlowFactory = Substitute.For<IPowerFlowFactory>();

    [Fact]
    public void CanCreateObject()
    {
        var cut = CreateTestObject();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void GetShouldReturnPowerProfile()
    {
        var cut = CreateTestObject();

        var act = () => cut.Execute(DateTime.Now);

        act.Should().NotThrow();
    }

    [Fact]
    public void SaveDataShouldUseRepository()
    {
        var cut = CreateTestObject();
        _houseBatteryService.GetConsumption().Returns(1);
        _houseBatteryService.GetProduction().Returns(2);
        _houseBatteryService.GetGridFeedIn().Returns(3);

        cut.SaveData(DateTime.Now);

        _realTimeReadingRepository.Received(1).Save(Arg.Any<DateTime>(), 1, 2, 3);
    }

    [Fact]
    public void UpdateModelShouldCallFitModel()
    {
        var now = DateTime.Parse("2022-02-21T09:00:00");
        var hour = now.AddHours(-1);
        _weatherService.GetHistory(Arg.Is(hour), Arg.Any<DateTime>()).Returns(new List<WeatherItem> { new () });
        _realTimeReadingRepository.GetConsumption(Arg.Is(hour)).Returns(1);
        _realTimeReadingRepository.GetProduction(Arg.Is(hour)).Returns(2);
        var cut = CreateTestObject();

        cut.UpdateModel(now);

        _predictConsumptionService.Received(1).FitModel(Arg.Any<ConsumptionData>(), Arg.Is(1));
        _predictProductionService.Received(1).FitModel(Arg.Any<ProductionData>(), Arg.Is(2));
    }

    [Fact]
    public void ExecuteWithChangeOfModeShouldSetBatteryMode()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var cut = CreateTestObject();
        _houseBatteryService.GetBatteryMode().Returns(BatteryMode.StoreForLater);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void ExecuteWithSameModeShouldSetBatteryMode()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var cut = CreateTestObject();
        _houseBatteryService.GetBatteryMode().Returns(BatteryMode.Default);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(Arg.Any<BatteryMode>(), Arg.Any<DateTime>());
    }

    private SmartPowerService CreateTestObject()
    {
        return new SmartPowerService(Options.Create(_options), _powerPriceService, _houseBatteryService, _productionService, _consumptionService, _realTimeReadingRepository, _logger, _predictConsumptionService, _predictProductionService, _weatherService, _powerUsageRepository, _powerMeterService, _powerFlowFactory);
    }
}