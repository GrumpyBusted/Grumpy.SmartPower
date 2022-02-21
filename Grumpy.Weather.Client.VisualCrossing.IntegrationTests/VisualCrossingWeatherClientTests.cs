using FluentAssertions;
using Grumpy.Common.Extensions;
using Grumpy.Rest;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Grumpy.Weather.Client.VisualCrossing.IntegrationTests;

public class VisualCrossingWeatherClientTests
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private readonly VisualCrossingWeatherClientOptions _options = new()
    {

        Latitude = 55.5763,
        Longitude = 12.2932,
        ApiKey = "2CPMKXLX4PYPSKU4W3TEK5CPP"
    };

    [Fact]
    public void CanGet()
    {
        var cut = CreateTestObject();

        var res = cut.Get(DateTime.Now.ToDateOnly());

        res.Should().HaveCount(24);
    }

    private IVisualCrossingWeatherClient CreateTestObject()
    {
        return new VisualCrossingWeatherClient(Options.Create(_options), new RestClientFactory(Substitute.For<ILoggerFactory>()));
    }
}