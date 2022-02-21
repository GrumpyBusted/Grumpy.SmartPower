using FluentAssertions;
using Grumpy.Rest;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Grumpy.Weather.Client.OpenWeatherMap.IntegrationTests;

public class OpenWeatherMapClientTests
{
    private readonly OpenWeatherMapClientOptions _options = new()
    {
        Latitude = 55.5763,
        Longitude = 12.2932,
        ApiKey = "b4afbfcc5aacca25a5345d0ed4d17dd3"
    };

    [Fact]
    public void CanGetSunInformation()
    {
        var cut = CreateTestObject();

        var res = cut.GetSunInformation();

        res.Sunrise.Should().BeBefore(res.Sunset);
    }

    [Fact]
    public void CanGetForecast()
    {
        var cut = CreateTestObject();

        var res = cut.GetForecast();

        res.Should().HaveCountGreaterThan(12);
    }

    private IOpenWeatherMapClient CreateTestObject()
    {
        return new OpenWeatherMapClient(Options.Create(_options), new RestClientFactory(Substitute.For<ILoggerFactory>()));
    }
}