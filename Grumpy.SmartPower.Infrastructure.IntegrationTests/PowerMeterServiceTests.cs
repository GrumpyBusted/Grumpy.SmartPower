using FluentAssertions;
using Grumpy.PowerMeter.Client.SmartMe;
using Grumpy.Rest;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Grumpy.Caching.TestMocks;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.IntegrationTests;

public class PowerMeterServiceTests
{
    private readonly SmartMePowerMeterClientOptions _options = new()
    {
        ApiToken = "YW5kZXJzQGJ1c3RlZC1qYW51bS5kazpKb0VtQ2EwMQ==",
        SerialNo = 9203930
    };

    [Fact]
    public void GetReadingShouldReturnValue()
    {
        var cut = CreateTestObject();

        var res = cut.GetReading(DateTime.Now);

        res.Should().BeGreaterThan(1);
    }

    private IPowerMeterService CreateTestObject()
    {
        var client = new SmartMePowerMeterClient(Options.Create(_options), new RestClientFactory(Substitute.For<ILoggerFactory>()));

        return new PowerMeterService(client, TestCacheFactory.Instance);
    }
}