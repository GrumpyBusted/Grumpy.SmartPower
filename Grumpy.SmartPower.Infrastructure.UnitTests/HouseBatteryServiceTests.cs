using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.SmartPower.Core.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Grumpy.Caching.TestMocks;
using Grumpy.HouseBattery.Client.Sonnen.Dto;
using Grumpy.SmartPower.Core.Infrastructure;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class HouseBatteryServiceTests
{
    [Fact]
    public void GetBatteryModeWhenOperatingModeIsSelfConsumptionShouldBeDefault()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetOperatingMode().Returns(OperatingMode.SelfConsumption);

        var cut = CreateTestObject(client);

        var res = cut.GetBatteryMode();

        res.Should().Be(BatteryMode.Default);
    }

    [Fact]
    public void GetBatteryModeWhenOperatingModeIsTimeOfUseWithNoGridChargeShouldBeStoreForLater()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetOperatingMode().Returns(OperatingMode.TimeOfUse);
        client.GetSchedule().Returns(new List<TimeOfUseEvent> { new() { Watt = 0 } });

        var cut = CreateTestObject(client);

        var res = cut.GetBatteryMode();

        res.Should().Be(BatteryMode.StoreForLater);
    }

    [Fact]
    public void GetBatteryModeWhenOperatingModeIsTimeOfUseWithGridChargeShouldBeStoreForLater()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetOperatingMode().Returns(OperatingMode.TimeOfUse);
        client.GetSchedule().Returns(new List<TimeOfUseEvent> { new() { Watt = 1 } });

        var cut = CreateTestObject(client);

        var res = cut.GetBatteryMode();

        res.Should().Be(BatteryMode.ChargeFromGrid);
    }

    [Fact]
    public void IsBatteryFullWith100ShouldBeTrue()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetBatteryLevel().Returns(100);

        var cut = CreateTestObject(client);

        var res = cut.IsBatteryFull();

        res.Should().Be(true);
    }

    [Fact]
    public void IsBatteryFullWith94ShouldBeFalse()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetBatteryLevel().Returns(94);

        var cut = CreateTestObject(client);

        var res = cut.IsBatteryFull();

        res.Should().Be(false);
    }

    [Fact]
    public void GetBatterySizeShouldReturnFromClient()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetBatteryCapacity().Returns(100);
        client.GetBatteryLevel().Returns(40);

        var cut = CreateTestObject(client);

        var res = cut.GetBatterySize();

        res.Should().Be(250);
    }

    [Fact]
    public void GetBatteryCapacityShouldReturnFromClient()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetBatteryCapacity().Returns(123);

        var cut = CreateTestObject(client);

        var res = cut.GetBatteryCurrent();

        res.Should().Be(123);
    }

    [Fact]
    public void GetBatteryLevelShouldReturnFromClient()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetBatteryLevel().Returns(23);

        var cut = CreateTestObject(client);

        var res = cut.GetBatteryLevel();

        res.Should().Be(23);
    }

    [Fact]
    public void GetConsumptionShouldReturnFromClient()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetConsumption().Returns(123);

        var cut = CreateTestObject(client);

        var res = cut.GetConsumption();

        res.Should().Be(123);
    }

    [Fact]
    public void GetProductionShouldReturnFromClient()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetProduction().Returns(123);

        var cut = CreateTestObject(client);

        var res = cut.GetProduction();

        res.Should().Be(123);
    }

    [Fact]
    public void SetModeToDefaultShouldSetToSelfConsumption()
    {
        var client = Substitute.For<ISonnenBatteryClient>();

        var cut = CreateTestObject(client);

        cut.SetMode(BatteryMode.Default, DateTime.Now);

        client.Received(1).SetOperatingMode(Arg.Is(OperatingMode.SelfConsumption));
    }

    [Fact]
    public void SetModeToStoreShouldSetToTimeOfUseWithZeroFromGrid()
    {
        var client = Substitute.For<ISonnenBatteryClient>();

        var cut = CreateTestObject(client);

        cut.SetMode(BatteryMode.StoreForLater, DateTime.Parse("2022-02-15T12:00:00"));

        client.Received(1).SetOperatingMode(Arg.Is(OperatingMode.TimeOfUse));
        client.Received(1).SetSchedule(Arg.Is<IEnumerable<TimeOfUseEvent>>(x => x.All(i => i.Watt == 0)));
        client.Received(1).SetSchedule(Arg.Is<IEnumerable<TimeOfUseEvent>>(x => x.Count() == 1));
    }

    [Fact]
    public void SetModeToStoreShouldSetScheduleToHour()
    {
        var client = Substitute.For<ISonnenBatteryClient>();

        var cut = CreateTestObject(client);

        cut.SetMode(BatteryMode.StoreForLater, DateTime.Parse("2022-02-15T12:00:00"));

        client.Received(1).SetSchedule(Arg.Is<IEnumerable<TimeOfUseEvent>>(x => x.All(i => i.Start == "12:00" & i.End == "14:00")));
        client.Received(1).SetSchedule(Arg.Is<IEnumerable<TimeOfUseEvent>>(x => x.Count() == 1));
    }

    [Fact]
    public void SetModeToChargeFromGridShouldSetToTimeOfUseWithValueFromGrid()
    {
        var client = Substitute.For<ISonnenBatteryClient>();
        client.GetBatteryCapacity().Returns(100);
        client.GetBatteryLevel().Returns(40);

        var cut = CreateTestObject(client);

        cut.SetMode(BatteryMode.ChargeFromGrid, DateTime.Parse("2022-02-15T12:00:00"));

        client.Received(1).SetOperatingMode(Arg.Is(OperatingMode.TimeOfUse));
        client.Received(1).SetSchedule(Arg.Is<IEnumerable<TimeOfUseEvent>>(x => x.All(i => i.Watt > 0)));
        client.Received(1).SetSchedule(Arg.Is<IEnumerable<TimeOfUseEvent>>(x => x.Count() == 1));
    }

    private static IHouseBatteryService CreateTestObject(ISonnenBatteryClient client)
    {
        return new HouseBatteryService(client, TestCacheFactory.Instance);
    }
}