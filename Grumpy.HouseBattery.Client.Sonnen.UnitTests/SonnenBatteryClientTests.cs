using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.Rest.Interface;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestSharp;
using System;
using System.Collections.Generic;
using Grumpy.HouseBattery.Client.Sonnen.Dto;
using Xunit;

namespace Grumpy.HouseBattery.Client.Sonnen.UnitTests;

public class SonnenBatteryClientTests
{
    private readonly IRestClientFactory _restClientFactory = Substitute.For<IRestClientFactory>();
    private readonly IRestClient _restClient = Substitute.For<IRestClient>();
 
    public SonnenBatteryClientTests()
    {
        _restClientFactory.Instance(Arg.Any<string>()).Returns(_restClient);
    }

    [Fact]
    public void GetBatteryLevelShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetBatteryLevel();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetConsumptionShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetConsumption();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetProductionShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetProduction();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetBatteryCapacityShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetBatteryCapacity();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetBatterySizeShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetBatterySize();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetOperatingModeShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetOperatingMode();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetTimeOfUseScheduleShouldThrow()
    {
        var cut = CreateTestObject();

        var act = () => cut.GetSchedule();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ChangeOperatingModeShouldThrow()
    {
        var cut = CreateTestObject();

        cut.SetOperatingMode(OperatingMode.Manual);

        _restClient.Received().Execute(Arg.Any<RestRequest>());
    }

    [Fact]
    public void ChangeTimeOfUseScheduleShouldThrow()
    {
        var schedule = new List<TimeOfUseEvent>
        {
            new() { Start = "03:00", End = "04:00", Watt = 3000},
            new() { Start = "23:00", End = "00:00", Watt = 2000}
        };
        var cut = CreateTestObject();

        cut.SetSchedule(schedule);

        _restClient.Received().Execute(Arg.Any<RestRequest>());
    }

    private ISonnenBatteryClient CreateTestObject()
    {
        return new SonnenBatteryClient(Options.Create(new SonnenBatteryClientOptions()), _restClientFactory);
    }
}