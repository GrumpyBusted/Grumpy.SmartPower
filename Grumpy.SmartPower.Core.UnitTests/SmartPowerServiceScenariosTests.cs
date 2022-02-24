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

public class SmartPowerServiceScenariosTests
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

    [Fact]
    public void PriceScenarioA()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioB()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioC()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioD()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 1000, Price = 1 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.StoreForLater, now);
    }

    [Fact]
    public void PriceScenarioD2()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 1000, Price = 1 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }


    [Fact]
    public void PriceScenarioE()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioF()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 1 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioG()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 3000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioH()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioI()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 500);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioJ()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 500, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 },
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 500);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioK()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 500, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 },
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 500);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioL()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>();
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 500);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioM()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 2000, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioN()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 2000, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioO()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 1000, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioP()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 1000, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 1000, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioQ()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 1000, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioR()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 6000, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 7000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(5000, 0, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioS()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 2000, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(1500, 1000, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    private void UseTestData(DateTime now, IEnumerable<TestDataFlow> list)
    {
        var productionList = new List<ProductionItem>();
        var consumptionList = new List<ConsumptionItem>();
        var priceList = new List<PriceItem>();
        var hour = 0;

        foreach (var data in list)
        {
            productionList.Add(new ProductionItem { Hour = now.AddHours(hour), WattPerHour = data.Production });
            consumptionList.Add(new ConsumptionItem { Hour = now.AddHours(hour), WattPerHour = data.Consumption });
            priceList.Add(new PriceItem { Hour = now.AddHours(hour), Price = data.Price });

            hour++;
        }

        _productionService.Predict(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(productionList);
        _consumptionService.Predict(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(consumptionList);
        _powerPriceService.GetPrices(Arg.Any<PriceArea>(), Arg.Any<PriceArea>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(priceList);
    }

    private SmartPowerService CreateTestObject(int batterySize, int batteryLevel, int inverterLimit)
    {
        _houseBatteryService.GetBatterySize().Returns(batterySize);
        _houseBatteryService.GetBatteryCurrent().Returns(batteryLevel);
        _houseBatteryService.InverterLimit().Returns(inverterLimit);
        _houseBatteryService.GetBatteryMode().Returns(BatteryMode.Manual);

        return new SmartPowerService(Options.Create(_options), _powerPriceService, _houseBatteryService, _productionService, _consumptionService, _realTimeReadingRepository, _logger, _predictConsumptionService, _predictProductionService, _weatherService);
    }
}