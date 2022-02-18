using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Production;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Grumpy.SmartPower.Core.Dto;
using Xunit;

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

    private SmartPowerService CreateTestObject()
    {
        return new SmartPowerService(Options.Create(_options), _powerPriceService, _houseBatteryService, _productionService, _consumptionService);
    }
}