using FluentAssertions;
using Grumpy.Common;
using Grumpy.Rest;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.Weather.Client.OpenWeatherMap;
using Grumpy.Weather.Client.VisualCrossing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Xunit;
using Grumpy.Caching.TestMocks;

namespace Grumpy.SmartPower.Infrastructure.IntegrationTests;

public class WeatherServiceTests
{
    private readonly OpenWeatherMapClientOptions _openWeatherMapClientOptions = new()
    {
        Latitude = 55.5763,
        Longitude = 12.2932,
        ApiKey = "b4afbfcc5aacca25a5345d0ed4d17dd3"
    };

    private readonly VisualCrossingWeatherClientOptions _visualCrossingWeatherClientOptions = new()
    {
        Latitude = 55.5763,
        Longitude = 12.2932,
        // ReSharper disable twice StringLiteralTypo
        ApiKey = "2CPMKXLX4PYPSKU4W3TEK5CPP"
    };

    [Fact]
    public void GetSunInformationShouldReturnSunriseBeforeSunset()
    {
        var cut = CreateTestObject();

        var res = cut.GetSunInformation();

        res.Sunrise.Should().BeBefore(res.Sunset);
    }

    [Fact]
    public void GetForecastShouldReturnListWith24Items()
    {
        var cut = CreateTestObject();

        var res = cut.GetForecast(DateTime.Now, DateTime.Now.AddDays(1));

        res.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GetHistoryShouldReturnListWith24Items()
    {
        var cut = CreateTestObject();

        var res = cut.GetHistory(DateTime.Now.Date.AddDays(-1), DateTime.Now.Date.AddSeconds(-1));

        res.Should().HaveCount(24);
    }

    private IWeatherService CreateTestObject()
    {
        var openWeatherMapClient = new OpenWeatherMapClient(Options.Create(_openWeatherMapClientOptions), new RestClientFactory(Substitute.For<ILoggerFactory>()));
        var visualCrossingWeatherClient = new VisualCrossingWeatherClient(Options.Create(_visualCrossingWeatherClientOptions), new RestClientFactory(Substitute.For<ILoggerFactory>()));

        return new WeatherService(openWeatherMapClient, visualCrossingWeatherClient, new DateTimeProvider(), TestCacheFactory.Instance);
    }
}