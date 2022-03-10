using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using NSubstitute;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests;

public class PowerHourTests
{
    private readonly IHouseBatteryService _houseBatteryService = Substitute.For<IHouseBatteryService>();

    [Fact]
    public void MaxChargeWithInverterLimitShouldReturnInverterLimit()
    {
        var cut = CreateTestObject(3300);

        var res = cut.MaxCharge();

        res.Should().Be(3300);
    }

    [Fact]
    public void MaxChargeWithAlreadyChargedShouldReturnUpToInverterLimit()
    {
        var cut = CreateTestObject(3300);
        cut.Charge = 2000;

        var res = cut.MaxCharge();

        res.Should().Be(1300);
    }

    [Fact]
    public void MaxChargeWithPreviouslyDischargedShouldReturnUpToInverterLimit()
    {
        var cut = CreateTestObject(3300);
        cut.Charge = -1000;

        var res = cut.MaxCharge();

        res.Should().Be(4300);
    }

    [Fact]
    public void MaxChargeWithBatterySizeShouldReturnBatterySize()
    {
        var cut = CreateTestObject(3300, 1000);

        var res = cut.MaxCharge();

        res.Should().Be(1000);
    }

    [Fact]
    public void MaxChargeWithAlreadyChargedShouldReturnUpToBatterySize()
    {
        var cut = CreateTestObject(3300, 1000);
        cut.Charge = 200;

        var res = cut.MaxCharge();

        res.Should().Be(800);
    }

    [Fact]
    public void MaxChargeWithStartingBatteryLevelShouldReturnUpToBatterySize()
    {
        var cut = CreateTestObject(3300, 1000);
        cut.BatteryLevel = 300;

        var res = cut.MaxCharge();

        res.Should().Be(700);
    }

    [Fact]
    public void MaxChargeWithStartingBatteryLevelAndAlreadyChargedShouldReturnUpToBatterySize()
    {
        var cut = CreateTestObject(3300, 1000);
        cut.BatteryLevel = 300;
        cut.Charge = 100;

        var res = cut.MaxCharge();

        res.Should().Be(600);
    }

    [Fact]
    public void MaxDischargeWithInverterLimitShouldReturnInverterLimit()
    {
        var cut = CreateTestObject(3300);
        cut.Power = -5000;
        cut.BatteryLevel = 5000;

        var res = cut.MaxDischarge();

        res.Should().Be(3300);
    }

    [Fact]
    public void MaxDischargeWithNothingBatteryLevelShouldReturnZero()
    {
        var cut = CreateTestObject();
        cut.Power = -5000;
        cut.BatteryLevel = 0;

        var res = cut.MaxDischarge();

        res.Should().Be(0);
    }

    [Fact]
    public void MaxDischargeWithLowBatteryLevelShouldReturnLow()
    {
        var cut = CreateTestObject();
        cut.Power = -5000;
        cut.BatteryLevel = 100;

        var res = cut.MaxDischarge();

        res.Should().Be(100);
    }

    [Fact]
    public void MaxDischargeWhenAlreadyDischargedShouldReturnUpToInverterLimit()
    {
        var cut = CreateTestObject(3300);
        cut.Power = -5000;
        cut.Charge = -900;
        cut.BatteryLevel = 5000;

        var res = cut.MaxDischarge();

        res.Should().Be(2400);
    }

    [Fact]
    public void MaxDischargeWhenAlreadyChargedShouldReturnUpToInverterLimitPlusCharged()
    {
        var cut = CreateTestObject(3300);
        cut.Power = -5000;
        cut.Charge = 900;
        cut.BatteryLevel = 9000;

        var res = cut.MaxDischarge();

        res.Should().Be(4200);
    }

    [Fact]
    public void MaxDischargeWhenAlreadyChargedShouldReturnUpToPreviousBatteryLevel()
    {
        var cut = CreateTestObject();
        cut.Power = -5000;
        cut.Charge = 900;
        cut.BatteryLevel = 1000;

        var res = cut.MaxDischarge();

        res.Should().Be(100);
    }

    [Fact]
    public void MaxDischargeWithNoMissingPowerShouldReturnZero()
    {
        var cut = CreateTestObject();
        cut.Power = 500;
        cut.BatteryLevel = 5000;

        var res = cut.MaxDischarge();

        res.Should().Be(0);
    }

    [Fact]
    public void MaxDischargeWithLowMissingPowerShouldReturnMissingPower()
    {
        var cut = CreateTestObject(3300);
        cut.BatteryLevel = 5000;
        cut.Power = -100;

        var res = cut.MaxDischarge();

        res.Should().Be(100);
    }

    private PowerHour CreateTestObject(int inverterLimit = 1000, int batterySize = 10000)
    {
        _houseBatteryService.InverterLimit().Returns(inverterLimit);
        _houseBatteryService.GetBatterySize().Returns(batterySize);

        var cut = new PowerHour(_houseBatteryService);
        return cut;
    }
}