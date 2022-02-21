using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.IntegrationTests;

public class SolarServiceTests
{
    private readonly SolarServiceOptions _options = new()
    {
        Latitude = 55.5763,
        Longitude = 12.2932
    };

    [Fact]
    public void DirectionShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Direction(DateTime.Parse("2022-02-13T12:00:00"));

        res.Should().BeApproximately(181.29, 0.1);
    }

    [Fact]
    public void AltitudeShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Altitude(DateTime.Parse("2022-02-13T12:00:00"));

        res.Should().BeApproximately(20.77, 0.1);
    }

    [Fact]
    public void SunlightShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunlight(DateTime.Parse("2022-02-13T12:00:00"));

        res.Should().Be(TimeSpan.FromMinutes(60));
    }

    private ISolarService CreateTestObject()
    {
        var library = new SolarInformation.SolarInformation();

        return new SolarService(Options.Create(_options), library);
    }
}