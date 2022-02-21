using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen;
using Grumpy.Rest;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.IntegrationTests;

public class HouseBatteryServiceTests
{
    private readonly SonnenBatteryClientOptions _options = new()
    {
        Ip = "192.168.0.222",
        ApiToken = "0ca39846-405a-4c8f-b48c-41f46fe17be1"
    };

    [Fact]
    public void GetBatteryLevelShouldReturnFromClient()
    {
        var cut = CreateTestObject();

        cut.IsBatteryFull();
    }

    [Fact]
    public void GetBatterySizeShouldReturnFromClient()
    {
        var cut = CreateTestObject();

        var res = cut.GetBatterySize();

        res.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetBatteryCapacityShouldReturnFromClient()
    {
        var cut = CreateTestObject();

        var res = cut.GetBatteryCurrent();

        res.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CanSetBatteryOperatingModeShouldWork()
    {
        var cut = CreateTestObject();

        cut.SetMode(BatteryMode.Default, DateTime.Parse("2022-02-15T12:00:00"));
    }

    private IHouseBatteryService CreateTestObject()
    {
        var client = new SonnenBatteryClient(Options.Create(_options), new RestClientFactory(Substitute.For<ILoggerFactory>()));

        return new HouseBatteryService(client);
    }
}