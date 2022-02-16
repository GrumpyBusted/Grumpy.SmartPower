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
    }
}