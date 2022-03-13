using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.TestTools.Extensions;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests;

public class PowerHourTests
{
    [Fact]
    public void CreateObjectShouldSetHour()
    {
        var cut = CreateTestObject(hour: DateTime.Parse("2022-02-13T10:00:00"));

        cut.Hour.Should().Be("2022-02-13T10:00:00");
    }

    [Fact]
    public void CreateObjectShouldSetProduction()
    {
        var cut = CreateTestObject(production: 10);

        cut.Production.Should().Be(10);
    }

    [Fact]
    public void CreateObjectShouldSetConsumption()
    {
        var cut = CreateTestObject(consumption: 10);

        cut.Consumption.Should().Be(10);
    }

    [Fact]
    public void CreateObjectShouldCalculatePower()
    {
        var cut = CreateTestObject(production: 100, consumption: 10);

        cut.Power.Should().Be(90);
    }

    [Fact]
    public void CreateObjectShouldSetPrice()
    {
        var cut = CreateTestObject(price: 10);

        cut.Price.Should().Be(10);
    }

    [Fact]
    public void CreateObjectShouldSetBatteryLevel()
    {
        var cut = CreateTestObject(batterySize: 1000, batteryLevel: 1000);

        cut.BatteryLevel.Should().Be(1000);
    }

    [Fact]
    public void CreateObjectWithInvalidHourShouldThrow()
    {
        var act = new Action(() => CreateTestObject(hour: DateTime.Parse("2022-02-13T10:00:01")));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateObjectWithInvalidProductionShouldThrow()
    {
        var act = new Action(() => CreateTestObject(production: -1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateObjectWithInvalidConsumptionShouldThrow()
    {
        var act = new Action(() => CreateTestObject(consumption: -1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateObjectWithInvalidBatteryLevelShouldThrow()
    {
        var act = new Action(() => CreateTestObject(batteryLevel: -1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateObjectWithBatteryLevelHigherThanSizeShouldThrow()
    {
        var act = new Action(() => CreateTestObject(batterySize: 1000, batteryLevel: 1001));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

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
        cut.ChargeBattery(2000);

        var res = cut.MaxCharge();

        res.Should().Be(1300);
    }

    [Fact]
    public void MaxChargeWithPreviouslyDischargedShouldReturnUpToInverterLimit()
    {
        var cut = CreateTestObject(inverterLimit: 3300, batteryLevel: 5000, consumption: 5000);
        cut.DischargeBattery(1000);

        var res = cut.MaxCharge();

        res.Should().Be(4300);
    }

    [Fact]
    public void MaxChargeWithBatterySizeShouldReturnBatterySize()
    {
        var cut = CreateTestObject(batterySize: 1000);

        var res = cut.MaxCharge();

        res.Should().Be(1000);
    }

    [Fact]
    public void MaxChargeWithAlreadyChargedShouldReturnUpToBatterySize()
    {
        var cut = CreateTestObject(batterySize: 1000);
        cut.ChargeBattery(200);

        var res = cut.MaxCharge();

        res.Should().Be(800);
    }

    [Fact]
    public void MaxChargeWithStartingBatteryLevelShouldReturnUpToBatterySize()
    {
        var cut = CreateTestObject(batterySize: 1000, batteryLevel: 300);

        var res = cut.MaxCharge();

        res.Should().Be(700);
    }

    [Fact]
    public void MaxChargeWithStartingBatteryLevelAndAlreadyChargedShouldReturnUpToBatterySize()
    {
        var cut = CreateTestObject(batterySize: 1000, batteryLevel: 200);
        cut.ChargeBattery(100);

        var res = cut.MaxCharge();

        res.Should().Be(700);
    }

    [Fact]
    public void MaxDischargeWithInverterLimitShouldReturnInverterLimit()
    {
        var cut = CreateTestObject(inverterLimit: 3300, batteryLevel: 5000, consumption: 5000);

        var res = cut.MaxDischarge();

        res.Should().Be(3300);
    }

    [Fact]
    public void MaxDischargeWithNothingBatteryLevelShouldReturnZero()
    {
        var cut = CreateTestObject(batteryLevel: 0, consumption: 5000);

        var res = cut.MaxDischarge();

        res.Should().Be(0);
    }

    [Fact]
    public void MaxDischargeWithLowBatteryLevelShouldReturnLow()
    {
        var cut = CreateTestObject(batteryLevel: 100, consumption: 5000);

        var res = cut.MaxDischarge();

        res.Should().Be(100);
    }

    [Fact]
    public void MaxDischargeWhenAlreadyDischargedShouldReturnUpToInverterLimit()
    {
        var cut = CreateTestObject(inverterLimit: 3300, batteryLevel: 5900, consumption: 5000);
        cut.DischargeBattery(900);

        var res = cut.MaxDischarge();

        res.Should().Be(2400);
    }

    [Fact]
    public void MaxDischargeWhenAlreadyChargedShouldReturnUpToInverterLimitPlusCharged()
    {
        var cut = CreateTestObject(inverterLimit: 3300, batteryLevel: 8100, consumption: 5000);
        cut.ChargeBattery(900);

        var res = cut.MaxDischarge();

        res.Should().Be(4200);
    }

    [Fact]
    public void MaxDischargeWhenAlreadyChargedShouldReturnUpToPreviousBatteryLevel()
    {
        var cut = CreateTestObject(batteryLevel: 1000, consumption: 5000);
        cut.ChargeBattery(900);

        var res = cut.MaxDischarge();

        res.Should().Be(100);
    }

    [Fact]
    public void MaxDischargeWithNoMissingPowerShouldReturnZero()
    {
        var cut = CreateTestObject(batteryLevel: 1000, production: 500);

        var res = cut.MaxDischarge();

        res.Should().Be(0);
    }

    [Fact]
    public void MaxDischargeWithLowMissingPowerShouldReturnMissingPower()
    {
        var cut = CreateTestObject(batteryLevel: 5000, consumption: 100);

        var res = cut.MaxDischarge();

        res.Should().Be(100);
    }

    [Fact]
    public void UseGridWhenMissingPowerShouldSetGridToNegative()
    {
        var cut = CreateTestObject(consumption: 100);

        cut.UseGrid();

        cut.Power.Should().Be(0);
        cut.Grid.Should().Be(-100);
    }

    [Fact]
    public void UseGridWhenExtraPowerShouldSetGridToPositive()
    {
        var cut = CreateTestObject(production: 100);

        cut.UseGrid();

        cut.Power.Should().Be(0);
        cut.Grid.Should().Be(100);
    }

    [Fact]
    public void UseGridWhenNoPowerShouldSetGridToZero()
    {
        var cut = CreateTestObject();

        cut.UseGrid();

        cut.Power.Should().Be(0);
        cut.Grid.Should().Be(0);
    }

    [Fact]
    public void ChargeWithInvalidValueShouldThrow()
    {
        var cut = CreateTestObject();

        var act = new Action(() => cut.ChargeBattery(-1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ChargeBatteryShouldIncreaseCharge()
    {
        var cut = CreateTestObject();
        cut.ChargeBattery(100);

        cut.ChargeBattery(200);

        cut.Charge.Should().Be(300);
    }

    [Fact]
    public void ChargeBatteryWhenNoExtraPowerShouldSetGridToNegative()
    {
        var cut = CreateTestObject();

        cut.ChargeBattery(100);

        cut.Grid.Should().Be(-100);
    }

    [Fact]
    public void ChargeBatteryWhenNoExtraPowerShouldNotChangePower()
    {
        var cut = CreateTestObject();

        cut.ChargeBattery(100);

        cut.Power.Should().Be(0);
    }

    [Fact]
    public void ChargeBatteryWhenMissingPowerShouldSetGridToNegative()
    {
        var cut = CreateTestObject(consumption: 100);

        cut.ChargeBattery(200);

        cut.Grid.Should().Be(-200);
    }

    [Fact]
    public void ChargeBatteryWhenMissingPowerShouldNotChangePower()
    {
        var cut = CreateTestObject(consumption: 100);

        cut.ChargeBattery(200);

        cut.Power.Should().Be(-100);
    }

    [Fact]
    public void ChargeBatteryWhenLotsOfExtraPowerShouldNotChargeGrid()
    {
        var cut = CreateTestObject(production: 1000);

        cut.ChargeBattery(200);

        cut.Grid.Should().Be(0);
    }

    [Fact]
    public void ChargeBatteryWhenLotsOfExtraPowerShouldTakeFromPower()
    {
        var cut = CreateTestObject(production: 1000);

        cut.ChargeBattery(200);

        cut.Power.Should().Be(800);
    }

    [Fact]
    public void ChargeBatteryWhenLittleExtraPowerShouldChangeGridWithMissingPower()
    {
        var cut = CreateTestObject(production: 110);

        cut.ChargeBattery(200);

        cut.Grid.Should().Be(-90);
    }

    [Fact]
    public void ChargeBatteryWhenLittleExtraPowerShouldTakeAllPower()
    {
        var cut = CreateTestObject(production: 110);

        cut.ChargeBattery(200);

        cut.Power.Should().Be(0);
    }

    [Fact]
    public void DischargeWithInvalidValueShouldThrow()
    {
        var cut = CreateTestObject();

        var act = new Action(() => cut.DischargeBattery(-1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DischargeBatteryShouldReducePower()
    {
        var cut = CreateTestObject(batteryLevel: 1000, consumption: 100);

        cut.DischargeBattery(90);

        cut.Power.Should().Be(-10);
    }

    [Fact]
    public void DischargeBatteryShouldReduceCharge()
    {
        var cut = CreateTestObject(batteryLevel: 1000, consumption: 100);

        cut.DischargeBattery(90);

        cut.Charge.Should().Be(-90);
    }

    [Fact]
    public void AdjustBatteryLevelToBelowZeroShouldThrow()
    {
        var cut = CreateTestObject(batteryLevel: 1000);

        var act = new Action(() => cut.AdjustBatteryLevel(-1001));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AdjustBatteryLevelToAboveSizeShouldThrow()
    {
        var cut = CreateTestObject(batterySize: 1000);

        var act = new Action(() => cut.AdjustBatteryLevel(1001));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AdjustBatteryLevelAddShouldSetBatteryLevel()
    {
        var cut = CreateTestObject();

        cut.AdjustBatteryLevel(100);

        cut.BatteryLevel.Should().Be(100);
    }

    [Fact]
    public void AdjustBatteryLevelSubscribeShouldSetBatteryLevel()
    {
        var cut = CreateTestObject(batteryLevel: 100);

        cut.AdjustBatteryLevel(-20);

        cut.BatteryLevel.Should().Be(80);
    }

    private static PowerHour CreateTestObject(int inverterLimit = 1000, int batterySize = 10000, int batteryLevel = 0, int production = 0, int consumption = 0, double price = 0, DateTime? hour = null)
    {
        var houseBatteryService = Substitute.For<IHouseBatteryService>();
        houseBatteryService.InverterLimit().Returns(inverterLimit);
        houseBatteryService.GetBatterySize().Returns(batterySize);

        return new PowerHour(houseBatteryService, hour ?? DateTime.Parse("2022-02-13T10:00:00"), production, consumption, price, batteryLevel);
    }
}