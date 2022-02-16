using FluentAssertions;
using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.Rest.Interface;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.PowerMeter.Client.SmartMe.UnitTests
{
    public class SmartMePowerMeterClientTests
    {
        [Fact]
        public void GetPricesShouldThrow()
        {
            var cut = CreateTestObject();

            var act = () => cut.GetValue(DateTime.Now);

            act.Should().Throw<Exception>();
        }

        private static ISmartMePowerMeterClient CreateTestObject()
        {
            return new SmartMePowerMeterClient(Options.Create(new SmartMePowerMeterClientOptions()), Substitute.For<IRestClientFactory>());
        }
    }
}