using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using Grumpy.TestTools.Extensions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests
{
    public class PowerFlowServiceTests
    {
        private readonly IHouseBatteryService _houseBatteryService = Substitute.For<IHouseBatteryService>();
        private readonly IPowerFlowFactory _powerFlowFactory;

        public PowerFlowServiceTests()
        {
            _powerFlowFactory = new PowerFlowFactory(_houseBatteryService);
        }

        [Fact]
        public void DistributeExtraPowerUnderBatterySizeShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 3);
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(200, 100, 1);
            var cut = CreateTestObject(batteryLevel: 1000);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.ChargeExtraPower(flow);

            flow.Skip(0).First().BatteryLevel.Should().Be(1000);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(0).First().Power.Should().Be(-100);
            flow.Skip(1).First().BatteryLevel.Should().Be(1000);
            flow.Skip(1).First().Charge.Should().Be(0);
            flow.Skip(1).First().Power.Should().Be(-100);
            flow.Skip(2).First().BatteryLevel.Should().Be(1100);
            flow.Skip(2).First().Charge.Should().Be(100);
            flow.Skip(2).First().Power.Should().Be(0);
        }

        [Fact]
        public void DistributeExtraPowerOverBatterySizeShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 3);
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(200, 100, 1);
            var cut = CreateTestObject(batterySize: 1000, batteryLevel: 1000) ;
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.DistributeExtraPower(flow);

            flow.Skip(0).First().BatteryLevel.Should().Be(900);
            flow.Skip(0).First().Charge.Should().Be(-100);
            flow.Skip(0).First().Power.Should().Be(0);
            flow.Skip(1).First().BatteryLevel.Should().Be(900);
            flow.Skip(1).First().Charge.Should().Be(0);
            flow.Skip(1).First().Power.Should().Be(-100);
            flow.Skip(2).First().BatteryLevel.Should().Be(1000);
            flow.Skip(2).First().Charge.Should().Be(100);
            flow.Skip(2).First().Power.Should().Be(0);
        }

        [Fact]
        public void DistributeExtraPowerOverBatterySizeToTwoHoursShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 3);
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(250, 100, 1);
            var cut = CreateTestObject(batterySize: 1000, batteryLevel: 1000);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.DistributeExtraPower(flow);

            flow.Skip(0).First().BatteryLevel.Should().Be(900);
            flow.Skip(0).First().Charge.Should().Be(-100);
            flow.Skip(0).First().Power.Should().Be(0);
            flow.Skip(1).First().BatteryLevel.Should().Be(850);
            flow.Skip(1).First().Charge.Should().Be(-50);
            flow.Skip(1).First().Power.Should().Be(-50);
            flow.Skip(2).First().BatteryLevel.Should().Be(1000);
            flow.Skip(2).First().Charge.Should().Be(150);
            flow.Skip(2).First().Power.Should().Be(0);
        }

        [Fact]
        public void DistributeInitialBatteryPowerShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 3);
            var cut = CreateTestObject(batteryLevel: 100);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.DistributeInitialBatteryPower(flow);

            flow.Skip(0).First().BatteryLevel.Should().Be(0);
            flow.Skip(0).First().Charge.Should().Be(-100);
            flow.Skip(0).First().Power.Should().Be(0);
            flow.Skip(1).First().BatteryLevel.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(0);
            flow.Skip(1).First().Power.Should().Be(-100);
            flow.Skip(2).First().BatteryLevel.Should().Be(0);
            flow.Skip(2).First().Charge.Should().Be(0);
            flow.Skip(2).First().Power.Should().Be(-100);
        }

        [Fact]
        public void DistributeInitialBatteryTwoPeeksPowerShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 4);
            testPowerFlow.Add(0, 100, 3);
            testPowerFlow.Add(0, 100, 5);
            var cut = CreateTestObject(batteryLevel: 200);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.DistributeInitialBatteryPower(flow);

            flow.Skip(0).First().Charge.Should().Be(-100);
            flow.Skip(1).First().Charge.Should().Be(0);
            flow.Skip(2).First().Charge.Should().Be(-100);
            flow.Skip(3).First().Charge.Should().Be(0);
            flow.Skip(4).First().Charge.Should().Be(0);
        }

        [Fact]
        public void ChargeFromGridMoveFromOneTotheNextShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 2);
            var cut = CreateTestObject();
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.ChargeFromGrid(flow);

            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(100);
            flow.Skip(2).First().Charge.Should().Be(-100);
        }

        [Fact]
        public void ChargeFromGridMoveButLimitToInverterLimitShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 2);
            var cut = CreateTestObject(inverterLimit: 50);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.ChargeFromGrid(flow);

            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(50);
            flow.Skip(2).First().Charge.Should().Be(-50);
        }

        [Fact]
        public void ChargeFromGridMoveFromTwoToTwoShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(0, 50, 3);
            var cut = CreateTestObject(inverterLimit: 100);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.ChargeFromGrid(flow);

            flow.Skip(0).First().Charge.Should().Be(50);
            flow.Skip(1).First().Charge.Should().Be(100);
            flow.Skip(2).First().Charge.Should().Be(-100);
            flow.Skip(3).First().Charge.Should().Be(-50);
        }

        [Fact]
        public void ChargeFromGridMoveFromOneToTwoShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 1);
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(0, 100, 2);
            var cut = CreateTestObject(inverterLimit: 200);
            var flow = testPowerFlow.PowerFlow(_houseBatteryService);

            cut.ChargeFromGrid(flow);

            flow.Skip(0).First().Charge.Should().Be(200);
            flow.Skip(1).First().Charge.Should().Be(-100);
            flow.Skip(2).First().Charge.Should().Be(-100);
        }

        private IPowerFlowService CreateTestObject(int batterySize = 10000, int inverterLimit = 3300, int batteryLevel = 0)
        {
            _houseBatteryService.GetBatterySize().Returns(batterySize);
            _houseBatteryService.InverterLimit().Returns(inverterLimit);
            _houseBatteryService.GetBatteryCurrent().Returns(batteryLevel);

            return new PowerFlowService(_houseBatteryService, _powerFlowFactory);
        }

        private IPowerFlowService CreateTestObject(TestPowerFlow testPowerFlow, int batterySize, int inverterLimit, int batteryLevel)
        {
            return CreateTestObject(testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices, testPowerFlow.Start, testPowerFlow.End, batterySize, inverterLimit, batteryLevel);
        }

        private IPowerFlowService CreateTestObject(IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices, DateTime from, DateTime to, int batterySize, int inverterLimit, int batteryLevel)
        {
            _houseBatteryService.GetBatterySize().Returns(batterySize);
            _houseBatteryService.InverterLimit().Returns(inverterLimit);
            _houseBatteryService.GetBatteryCurrent().Returns(batteryLevel);

            return new PowerFlowService(_houseBatteryService, _powerFlowFactory);
        }
    }
}
