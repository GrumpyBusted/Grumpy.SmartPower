using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests
{
    public class PowerFlowFactoryTests
    {
        private readonly IHouseBatteryService _houseBatteryService = Substitute.For<IHouseBatteryService>();

        [Fact]
        public void InstanceWithDataShouldReturnPowerFlow()
        {
            var cut = CreateTestObject();

            var res = cut.Instance(DateTime.Parse("2022-02-13T10:00:00"), Substitute.For<IList<ProductionItem>>(), Substitute.For<IList<ConsumptionItem>>(), Substitute.For<IList<PriceItem>>());

            res.Should().BeOfType<PowerFlow>();
        }

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
