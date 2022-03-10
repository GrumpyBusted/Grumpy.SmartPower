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
    public class PowerFlowTests
    {
        private readonly IHouseBatteryService _houseBatteryService = Substitute.For<IHouseBatteryService>();
        private readonly IPowerFlowFactory _powerFlowFactory;

        public PowerFlowTests()
        {
            _powerFlowFactory = new PowerFlowFactory(_houseBatteryService);
        }

        [Fact]
        public void FirstOnEmptyShouldReturnNull()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.First();

            res.Should().Be(null);
        }

        [Fact]
        public void FirstShouldReturnFirstItem()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(1, 0, 0);
            testPowerFlow.Add(2, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.First();

            res?.Production.Should().Be(1);
        }

        [Fact]
        public void GetOnEmptyShouldReturnNull()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.Get(DateTime.Parse("2022-02-13T10:00:00"));

            res?.Production.Should().Be(2);
        }

        [Fact]
        public void GetShouldReturnItem()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(1, 0, 0);
            testPowerFlow.Add(2, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.Get(DateTime.Parse("2022-02-13T10:00:00"));

            res?.Production.Should().Be(2);
        }

        [Fact]
        public void GetNonExistingShouldReturnNull()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(1, 0, 0);
            testPowerFlow.Add(2, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.Get(DateTime.Parse("2022-02-13T11:00:00"));

            res?.Production.Should().Be(null);
        }

        [Fact]
        public void InvalidFromWhenCreatingShouldThrow()
        {
            var act = () => CreateTestObject(Enumerable.Empty<ProductionItem>(), Enumerable.Empty<ConsumptionItem>(), Enumerable.Empty<PriceItem>(), DateTime.Parse("2022-02-13T09:01:00"), DateTime.Parse("2022-02-13T09:01:00"), 0, 0, 0);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void CreateShouldSetBatteryLevelToAll()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 1, 0);
            testPowerFlow.Add(0, 1, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 1000);

            var res = cut.All();

            res.Should().OnlyContain(i => i.BatteryLevel == 1000);
        }

        [Fact]
        public void CreateShouldSetChargeToAll()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(1, 2, 3);
            testPowerFlow.Add(4, 5, 6);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 1000);

            var res = cut.All();

            res.Should().OnlyContain(i => i.Charge == 0);
        }

        [Fact]
        public void CreateShouldReturnListWithItemsAtEachHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 1000);

            var res = cut.All().OrderBy(i => i.Hour);

            res.Skip(0).First().Hour.Should().Be("2022-02-13T09:00:00");
            res.Skip(1).First().Hour.Should().Be("2022-02-13T10:00:00");
            res.Skip(2).First().Hour.Should().Be("2022-02-13T11:00:00");
        }

        [Fact]
        public void CreateWithThreeShouldReturnThreeItemsInFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.All();

            res.Should().HaveCount(3);
        }

        [Fact]
        public void CreateWithMissingProductionValueShouldReturnFlowUntilFirstMissingValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(null, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.All();

            res.Should().HaveCount(2);
        }

        [Fact]
        public void CreateWithMissingConsumptionValueShouldReturnFlowUntilFirstMissingValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, null, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.All();

            res.Should().HaveCount(1);
        }

        [Fact]
        public void CreateWithMissingPriceValueShouldReturnFlowUntilFirstMissingValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, null);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.All();

            res.Should().HaveCount(0);
        }

        [Fact]
        public void CreateShouldSetDataOnFirstItem()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(1, 2, 3);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 1000);

            var res = cut.All().OrderBy(i => i.Hour);

            res.First().Production.Should().Be(1);
            res.First().Consumption.Should().Be(2);
            res.First().Price.Should().Be(3);
        }

        [Fact]
        public void CreateShouldSetPowerToMissing()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(1, 4, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.All().OrderBy(i => i.Hour);

            res.First().Power.Should().Be(-3);
        }

        [Fact]
        public void CreateShouldSetPowerToExtra()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(4, 1, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var res = cut.All().OrderBy(i => i.Hour);

            res.First().Power.Should().Be(3);
        }

        [Fact]
        public void ChargeEmptyFlowShouldThrow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var act = () => cut.ChargeBattery(DateTime.Parse("2022-02-13T09:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChargeWhereHourNotFoundFlowShouldThrow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var act = () => cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChargeShouldChargeBatteryLevelAfterHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 10);

            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 30);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(40);
            res.Skip(2).First().BatteryLevel.Should().Be(40);
        }

        [Fact]
        public void ChargeBeyondSizeShouldChargeBatteryLevelAfterHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 40, 1000, 10);

            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 50);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(40);
            res.Skip(2).First().BatteryLevel.Should().Be(40);
        }

        [Fact]
        public void ChargeTwiceBeyondInverterLimitShouldChargeBatteryLevelAfterHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 30, 10);

            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);
            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(40);
            res.Skip(2).First().BatteryLevel.Should().Be(40);
        }

        [Fact]
        public void ChargeShouldReturnActualChargeValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 10);

            var res = cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 30);

            res.Should().Be(30);
        }

        [Fact]
        public void ChargeBeyondSizeShouldReturnActualChargeValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 40, 1000, 10);

            var res = cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 50);

            res.Should().Be(30);
        }

        [Fact]
        public void ChargeTwiceBeyondInverterLimitShouldReturnActualChargeValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 30, 10);

            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);
            var res = cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            res.Should().Be(10);
        }

        [Fact]
        public void ChargeShouldConsiderFutureBatteryLevel()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 50, 1000, 0);

            cut.ChargeBattery(DateTime.Parse("2022-02-13T11:00:00"), 20).Should().Be(20);
            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20).Should().Be(20);
            cut.ChargeBattery(DateTime.Parse("2022-02-13T09:00:00"), 20).Should().Be(10);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(30);
            res.Skip(2).First().BatteryLevel.Should().Be(50);
        }

        [Fact]
        public void ChargeShouldChangeCharge()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 0);

            cut.ChargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();

            res.Skip(0).First().Charge.Should().Be(0);
            res.Skip(1).First().Charge.Should().Be(20);
            res.Skip(2).First().Charge.Should().Be(0);
        }

        [Fact]
        public void DischargeEmptyFlowShouldThrow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var act = () => cut.DischargeBattery(DateTime.Parse("2022-02-13T09:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void DischargeWhereHourNotFoundFlowShouldThrow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 0, 0, 0);

            var act = () => cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void DischargeShouldChargeBatteryLevelAfterHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 1000);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 30);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(1000);
            res.Skip(1).First().BatteryLevel.Should().Be(970);
            res.Skip(2).First().BatteryLevel.Should().Be(970);
        }

        [Fact]
        public void DischargeBeyondZeroShouldChargeBatteryLevelAfterHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 40, 1000, 30);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 50);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(30);
            res.Skip(1).First().BatteryLevel.Should().Be(0);
            res.Skip(2).First().BatteryLevel.Should().Be(0);
        }

        [Fact]
        public void DischargeTwiceBeyondInverterLimitShouldChargeBatteryLevelAfterHour()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 30, 1000);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);
            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(1000);
            res.Skip(1).First().BatteryLevel.Should().Be(970);
            res.Skip(2).First().BatteryLevel.Should().Be(970);
        }

        [Fact]
        public void DischargeShouldReturnActualChargeValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 1000);

            var res = cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 30);

            res.Should().Be(30);
        }

        [Fact]
        public void DischargeBeyondZeroShouldReturnActualDishargeValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 30);

            var res = cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 50);

            res.Should().Be(30);
        }

        [Fact]
        public void DischargeTwiceBeyondInverterLimitShouldReturnActualDischargeValue()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 30, 1000);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);
            var res = cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            res.Should().Be(10);
        }

        [Fact]
        public void DischargeShouldConsiderFutureBatteryLevel()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 100, 0);

            var cut = CreateTestObject(testPowerFlow, 50, 1000, 50);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T11:00:00"), 20).Should().Be(20);
            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20).Should().Be(20);
            cut.DischargeBattery(DateTime.Parse("2022-02-13T09:00:00"), 20).Should().Be(10);

            var res = cut.All();

            res.Skip(0).First().BatteryLevel.Should().Be(40);
            res.Skip(1).First().BatteryLevel.Should().Be(20);
            res.Skip(2).First().BatteryLevel.Should().Be(00);
        }

        [Fact]
        public void DischargeShouldChangeCharge()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 50);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();

            res.Skip(0).First().Charge.Should().Be(0);
            res.Skip(1).First().Charge.Should().Be(-20);
            res.Skip(2).First().Charge.Should().Be(0);
        }

        [Fact]
        public void DischargeBeyondZeroShouldChangeChargeWithLimitation()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 10);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();

            res.Skip(0).First().Charge.Should().Be(0);
            res.Skip(1).First().Charge.Should().Be(-10);
            res.Skip(2).First().Charge.Should().Be(0);
        }

        [Fact]
        public void DischargeMoreThanNeedShouldDischargeNeed()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);
            testPowerFlow.Add(0, 0, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 1000);

            var res = cut.DischargeBattery(DateTime.Parse("2022-02-13T10:00:00"), 150);

            res.Should().Be(100);
        }

        [Fact]
        public void MovePowerShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 50);

            var res = cut.MovePower(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 10);

            res.Should().Be(10);

            var flow = cut.All();

            flow.Skip(0).First().BatteryLevel.Should().Be(50);
            flow.Skip(1).First().BatteryLevel.Should().Be(60);
            flow.Skip(2).First().BatteryLevel.Should().Be(50);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(10);
            flow.Skip(2).First().Charge.Should().Be(-10);
        }

        [Fact]
        public void MovePowerBeyondSizeShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);

            var cut = CreateTestObject(testPowerFlow, 80, 1000, 50);

            var res = cut.MovePower(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 40);

            res.Should().Be(30);

            var flow = cut.All();

            flow.Skip(0).First().BatteryLevel.Should().Be(50);
            flow.Skip(1).First().BatteryLevel.Should().Be(80);
            flow.Skip(2).First().BatteryLevel.Should().Be(50);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(30);
            flow.Skip(2).First().Charge.Should().Be(-30);
        }

        [Fact]
        public void MovePowerBeyondInverterLimitShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);

            var cut = CreateTestObject(testPowerFlow, 100, 20, 50);

            var res = cut.MovePower(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 40);

            res.Should().Be(20);

            var flow = cut.All();

            flow.Skip(0).First().BatteryLevel.Should().Be(50);
            flow.Skip(1).First().BatteryLevel.Should().Be(70);
            flow.Skip(2).First().BatteryLevel.Should().Be(50);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(20);
            flow.Skip(2).First().Charge.Should().Be(-20);
        }

        [Fact]
        public void MovePowerBeyondInverterOnDischargeLimitShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 0, 0);
            testPowerFlow.Add(0, 100, 0);

            var cut = CreateTestObject(testPowerFlow, 100, 40, 50);

            cut.DischargeBattery(DateTime.Parse("2022-02-13T11:00:00"), 15);
            var res = cut.MovePower(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 30);

            res.Should().Be(25);

            var flow = cut.All();

            flow.Skip(0).First().BatteryLevel.Should().Be(50);
            flow.Skip(1).First().BatteryLevel.Should().Be(75);
            flow.Skip(2).First().BatteryLevel.Should().Be(35);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(25);
            flow.Skip(2).First().Charge.Should().Be(-40);
        }

        [Fact]
        public void DistributeExtraPowerUnderBatterySizeShouldReturnFlow()
        {
            var testPowerFlow = new TestPowerFlow("2022-02-13T09:00:00");
            testPowerFlow.Add(0, 100, 3);
            testPowerFlow.Add(0, 100, 2);
            testPowerFlow.Add(200, 100, 1);

            var cut = CreateTestObject(testPowerFlow, 2000, 1000, 1000);

            cut.ChargeExtraPower();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 1000);

            cut.ChargeExtraPower();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 1000);

            cut.ChargeExtraPower();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 100);

            cut.DistributeInitialBatteryPower();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 200);

            cut.DistributeInitialBatteryPower();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 1000, 0);

            cut.ChargeFromGrid();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 50, 0);

            cut.ChargeFromGrid();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 100, 0);

            cut.ChargeFromGrid();

            var flow = cut.All();

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

            var cut = CreateTestObject(testPowerFlow, 1000, 200, 0);

            cut.ChargeFromGrid();

            var flow = cut.All();

            flow.Skip(0).First().Charge.Should().Be(200);
            flow.Skip(1).First().Charge.Should().Be(-100);
            flow.Skip(2).First().Charge.Should().Be(-100);
        }


        private IPowerFlow1 CreateTestObject(TestPowerFlow testPowerFlow, int batterySize, int inverterLimit, int batteryLevel)
        {
            return CreateTestObject(testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices, testPowerFlow.Start, testPowerFlow.End, batterySize, inverterLimit, batteryLevel);
        }

        private IPowerFlow1 CreateTestObject(IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices, DateTime from, DateTime to, int batterySize, int inverterLimit, int batteryLevel)
        {
            _houseBatteryService.GetBatterySize().Returns(batterySize);
            _houseBatteryService.InverterLimit().Returns(inverterLimit);
            _houseBatteryService.GetBatteryCurrent().Returns(batteryLevel);

            return _powerFlowFactory.Instance(productions, consumptions, prices, from, to);
        }
    }
}
