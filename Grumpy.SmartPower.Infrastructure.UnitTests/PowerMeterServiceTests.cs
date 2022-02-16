using FluentAssertions;
using Grumpy.PowerMeter.Client.SmartMe.Interface;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests
{
    public class PowerMeterServiceTests
    {
        [Fact]
        public void GetReadingShouldReturnFromClient()
        {
            var client = Substitute.For<ISmartMePowerMeterClient>();
            client.GetValue(Arg.Any<DateTime>()).Returns(123);

            var cut = new PowerMeterService(client);

            var res = cut.GetReading(DateTime.Now);

            res.Should().Be(123);
        }

        [Fact]
        public void GetUsageShouldReturnFromClient()
        {
            var client = Substitute.For<ISmartMePowerMeterClient>();
            client.GetValue(DateTime.Parse("2022-02-12T13:00:00")).Returns(300);
            client.GetValue(DateTime.Parse("2022-02-12T14:00:00")).Returns(400);

            var cut = new PowerMeterService(client);

            var res = cut.GetUsagePerHour(DateTime.Parse("2022-02-12T13:34:00"));

            res.Should().Be(100000);
        }
    }
}