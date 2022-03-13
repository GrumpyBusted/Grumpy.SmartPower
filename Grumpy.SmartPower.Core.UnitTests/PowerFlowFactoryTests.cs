using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using NSubstitute;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests
{
    public class PowerFlowFactoryTests
    {
        private readonly IHouseBatteryService _houseBatteryService = Substitute.For<IHouseBatteryService>();

        [Fact]
        public void InstanceShouldReturnPowerFlow()
        {
            var cut = CreateTestObject();

            var res = cut.Instance();

            res.Should().BeOfType<PowerFlow>();
        }

        private PowerFlowFactory CreateTestObject()
        {
            return new PowerFlowFactory(_houseBatteryService);
        }
    }
}
