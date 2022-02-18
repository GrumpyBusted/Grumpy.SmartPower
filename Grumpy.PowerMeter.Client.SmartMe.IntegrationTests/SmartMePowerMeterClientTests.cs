using FluentAssertions;
using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.PowerMeter.Client.SmartMe.IntegrationTests;

public class SmartMePowerMeterClientTests
{
    private readonly SmartMePowerMeterClientOptions _options = new()
    {
        ApiToken = "YW5kZXJzQGJ1c3RlZC1qYW51bS5kazpKb0VtQ2EwMQ==",
        SerialNo = 9203930
    };

    [Fact]
    public void GetValue()
    {
        var cut = CreateTestObject();

        var res = cut.GetValue(DateTime.Now);

        res.Should().BeGreaterThan(0.0);
    }

    private ISmartMePowerMeterClient CreateTestObject()
    {
        return new SmartMePowerMeterClient(Options.Create(_options), new RestClientFactory(Substitute.For<ILoggerFactory>()));
    }
}