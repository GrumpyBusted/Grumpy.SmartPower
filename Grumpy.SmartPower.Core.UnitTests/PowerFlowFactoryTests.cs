using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using NSubstitute;
using System;
using System.Linq;
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

            var res = cut.Instance(Enumerable.Empty<ProductionItem>(), Enumerable.Empty<ConsumptionItem>(), Enumerable.Empty<PriceItem>(), DateTime.Parse("2022-02-13T11:00:00"), DateTime.Parse("2022-02-13T13:00:00"));

            res.Should().BeOfType<PowerFlow1>();
        }

        private PowerFlowFactory CreateTestObject()
        {
            return new PowerFlowFactory(_houseBatteryService);
        }
    }
}
