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
using System.Linq;

namespace Grumpy.SmartPower.Core.UnitTests;

public class SmartPowerServiceScenariosTests
{
    private readonly SmartPowerServiceOptions _options = new()
    {
        PriceArea = PriceArea.DK2,
        TraceFilePath = "bin"
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
    private readonly IPowerFlowFactory _powerFlowFactory;

    public SmartPowerServiceScenariosTests()
    {
        _powerFlowFactory = new PowerFlowFactory(_houseBatteryService);
    }

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

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
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
            new() { Production = 0, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 0, 500);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }

    [Fact]
    public void PriceScenarioJ2()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1500, Price = 2 },
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 }
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
            new() { Production = 0, Consumption = 1000, Price = 2 }
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
        var testData = Enumerable.Empty<TestDataFlow>();
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
    public void PriceScenarioM2()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 2000, Consumption = 1000, Price = 1 },
            new() { Production = 2000, Consumption = 1000, Price = 2 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(1000, 0, 1000);

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

        _houseBatteryService.Received(1).SetMode(BatteryMode.StoreForLater, now);
    }

    [Fact]
    public void PriceScenarioN2()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1 },
            new() { Production = 0, Consumption = 1000, Price = 2 } 
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 2000, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
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

        _houseBatteryService.Received(1).SetMode(BatteryMode.StoreForLater, now);
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
        var cut = CreateTestObject(5000, 0, 10000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioR2()
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

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
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

    [Fact]
    public void PriceScenarioT()
    {
        var now = DateTime.Parse("2022-02-13T12:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 3000, Consumption = 1000, Price = 3 },
            new() { Production = 0, Consumption = 2000, Price = 3 },
            new() { Production = 0, Consumption = 1000, Price = 3 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(1500, 0, 1000);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioU()
    {
        var now = DateTime.Parse("2022-02-13T05:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 2222, Price = 1505 },
            new() { Production = 0, Consumption = 2222, Price = 1860 },
            new() { Production = 388, Consumption = 2222, Price = 1960 },
            new() { Production = 1865, Consumption = 2222, Price = 2050 },
            new() { Production = 3593, Consumption = 2222, Price = 1989 },
            new() { Production = 5008, Consumption = 2222, Price = 1828 },
            new() { Production = 3292, Consumption = 2222, Price = 1585 },
            new() { Production = 174, Consumption = 2222, Price = 1463 },
            new() { Production = 0, Consumption = 2222, Price = 1331 },
            new() { Production = 0, Consumption = 2222, Price = 1317 },
            new() { Production = 0, Consumption = 2222, Price = 1469 },
            new() { Production = 0, Consumption = 2222, Price = 1674 },
            new() { Production = 0, Consumption = 2222, Price = 1934 },
            new() { Production = 0, Consumption = 2222, Price = 2225 },
            new() { Production = 0, Consumption = 2222, Price = 2237 },
            new() { Production = 0, Consumption = 2222, Price = 2068 },
            new() { Production = 0, Consumption = 2222, Price = 1754 },
            new() { Production = 0, Consumption = 2222, Price = 1801 },
            new() { Production = 0, Consumption = 2222, Price = 1801 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 8713, 3300);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioV()
    {
        var now = DateTime.Parse("2022-02-13T05:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 945, Price = 1934 },
            new() { Production = 0, Consumption = 945, Price = 2225 },
            new() { Production = 0, Consumption = 945, Price = 2237 },
            new() { Production = 0, Consumption = 945, Price = 2068 },
            new() { Production = 0, Consumption = 945, Price = 1754 },
            new() { Production = 0, Consumption = 2184, Price = 1801 },
            new() { Production = 0, Consumption = 2040, Price = 1578 },
            new() { Production = 0, Consumption = 1822, Price = 1455 },
            new() { Production = 0, Consumption = 1389, Price = 1344 },
            new() { Production = 0, Consumption = 1831, Price = 1298 },
            new() { Production = 0, Consumption = 1885, Price = 1282 },
            new() { Production = 0, Consumption = 2222, Price = 1298 },
            new() { Production = 0, Consumption = 2255, Price = 1317 },
            new() { Production = 0, Consumption = 2195, Price = 1384 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 8713, 3300);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.Default, now);
    }

    [Fact]
    public void PriceScenarioV2()
    {
        var now = DateTime.Parse("2022-02-13T05:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 1000, Price = 1934 },
            new() { Production = 0, Consumption = 1000, Price = 2225 },
            new() { Production = 0, Consumption = 1000, Price = 2237 },
            new() { Production = 0, Consumption = 1000, Price = 2068 },
            new() { Production = 0, Consumption = 1000, Price = 1754 },
            new() { Production = 0, Consumption = 1000, Price = 1801 },
            new() { Production = 0, Consumption = 1000, Price = 1578 },
            new() { Production = 0, Consumption = 1000, Price = 1455 },
            new() { Production = 0, Consumption = 1000, Price = 1344 },
            new() { Production = 0, Consumption = 1000, Price = 1298 },
            new() { Production = 0, Consumption = 1000, Price = 1282 },
            new() { Production = 0, Consumption = 1000, Price = 1298 },
            new() { Production = 0, Consumption = 1000, Price = 1317 },
            new() { Production = 0, Consumption = 1000, Price = 1384 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 2000, 3300);

        cut.Execute(now);

        _houseBatteryService.Received(1).SetMode(BatteryMode.ChargeFromGrid, now);
    }


    [Fact]
    public void PriceScenarioW()
    {
        var now = DateTime.Parse("2022-02-13T05:00:00");
        var testData = new List<TestDataFlow>
        {
            new() { Production = 0, Consumption = 2585, Price = 1.3981 },
            new() { Production = 0, Consumption = 1936, Price = 1.2838 },
            new() { Production = 0, Consumption = 2063, Price = 1.1138 },
            new() { Production = 0, Consumption = 2028, Price = 1.2143 },
            new() { Production = 0, Consumption = 1534, Price = 1.0658 },
            new() { Production = 0, Consumption = 1925, Price = 1.0652 },
            new() { Production = 0, Consumption = 1883, Price = 1.0658 },
            new() { Production = 0, Consumption = 2255, Price = 1.4274 },
            new() { Production = 0, Consumption = 1868, Price = 1.3096 },
            new() { Production = 0, Consumption = 1630, Price = 1.9035 },
            new() { Production = 516, Consumption = 1175, Price = 2.4059 },
            new() { Production = 2082, Consumption = 402, Price = 2.2987 }
        };
        UseTestData(now, testData);
        var cut = CreateTestObject(10000, 1706, 3300);

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

        return new SmartPowerService(Options.Create(_options), _powerPriceService, _houseBatteryService, _productionService, _consumptionService, _realTimeReadingRepository, _logger, _predictConsumptionService, _predictProductionService, _weatherService, _powerUsageRepository, _powerMeterService, _powerFlowFactory);
    }
}