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
        public void AddListWithEmptyListShouldLeaveZeroItems()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().Should().HaveCount(0);
        }

        [Fact]
        public void AddListWithListShouldLeaveItems()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().Should().HaveCount(2);
        }

        [Fact]
        public void AddListWithListShouldSetHourOfFirstItemToStart()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().First().Hour.Should().Be("2022-02-13T10:00:00");
        }

        [Fact]
        public void AddListWithListShouldSetHourOfSecondItemToPlusOneHour()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().Skip(1).First().Hour.Should().Be("2022-02-13T11:00:00");
        }

        [Fact]
        public void AddListWithListShouldSetProduction()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().First().Production.Should().Be(1);
        }

        [Fact]
        public void AddListWithListShouldSetConsumption()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().First().Consumption.Should().Be(3);
        }

        [Fact]
        public void AddListWithListShouldSetPrice()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().First().Price.Should().Be(5);
        }

        [Fact]
        public void AddListWithNullInProductionShouldStopList()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(null, 4, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().Should().HaveCount(1);
        }

        [Fact]
        public void AddListWithNullInConsumptionShouldStopList()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, null, 6);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().Should().HaveCount(1);
        }

        [Fact]
        public void AddListWithNullInPriceShouldStopList()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);
            testPowerFlow.Add(2, 4, null);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices);

            cut.All().Should().HaveCount(1);
        }

        [Fact]
        public void AddListWithOffHourShouldThrow()
        {
            var cut = CreateTestObject();
            var testPowerFlow = new TestPowerFlow("2022-02-13T10:00:00");
            testPowerFlow.Add(1, 3, 5);

            var act = new Action(() => cut.Add(DateTime.Parse("2022-02-13T10:00:01"), testPowerFlow.Productions, testPowerFlow.Consumptions, testPowerFlow.Prices));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddListShouldSetBatteryLevelToAll()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batteryLevel: 1000);

            var res = cut.All();

            res.Should().OnlyContain(i => i.BatteryLevel == 1000);
        }

        [Fact]
        public void AddListShouldSetChargeToAll()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(1, 2, 3);
            testData.Add(4, 5, 6);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All();

            res.Should().OnlyContain(i => i.Charge == 0);
        }

        [Fact]
        public void AddListShouldReturnListWithItemsAtEachHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All().OrderBy(i => i.Hour);

            res.Skip(0).First().Hour.Should().Be("2022-02-13T09:00:00");
            res.Skip(1).First().Hour.Should().Be("2022-02-13T10:00:00");
            res.Skip(2).First().Hour.Should().Be("2022-02-13T11:00:00");
        }

        [Fact]
        public void AddListWithThreeShouldReturnThreeItemsInFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All();

            res.Should().HaveCount(3);
        }

        [Fact]
        public void AddListWithMissingProductionValueShouldReturnFlowUntilFirstMissingValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(null, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All();

            res.Should().HaveCount(2);
        }

        [Fact]
        public void AddListWithMissingConsumptionValueShouldReturnFlowUntilFirstMissingValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, null, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All();

            res.Should().HaveCount(1);
        }

        [Fact]
        public void AddListWithMissingPriceValueShouldReturnFlowUntilFirstMissingValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, null);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All();

            res.Should().HaveCount(0);
        }

        [Fact]
        public void AddListShouldSetDataOnFirstItem()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(1, 2, 3);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All().OrderBy(i => i.Hour);

            res.First().Production.Should().Be(1);
            res.First().Consumption.Should().Be(2);
            res.First().Price.Should().Be(3);
        }

        [Fact]
        public void AddListShouldSetPowerToMissing()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(1, 4, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All().OrderBy(i => i.Hour);

            res.First().Power.Should().Be(-3);
        }

        [Fact]
        public void AddListShouldSetPowerToExtra()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(4, 1, 0);
            var cut = CreateTestObject(testData: testData);

            var res = cut.All().OrderBy(i => i.Hour);

            res.First().Power.Should().Be(3);
        }

        [Fact]
        public void AddItemWithOffHourShouldThrow()
        {
            var cut = CreateTestObject();

            var act = new Action(() => cut.Add(DateTime.Parse("2022-02-13T10:00:01"), 0, 0, 0));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddItemInWrongOrderShouldThrow()
        {
            var cut = CreateTestObject();

            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0);
            var act = new Action(() => cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0));

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddItemWithSameHourShouldThrow()
        {
            var cut = CreateTestObject();

            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0);
            var act = new Action(() => cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0));

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddItemShouldSetBatteryLevelFromHouseBatteryService()
        {
            var cut = CreateTestObject(batteryLevel: 100);

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);

            cut.First()?.BatteryLevel.Should().Be(100);
        }

        [Fact]
        public void AddSecondItemShouldSetBatteryLevelFromPrevious()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);
            cut.First()?.AdjustBatteryLevel(100);

            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0);

            cut.Last()?.BatteryLevel.Should().Be(100);
        }

        [Fact]
        public void AddShouldSetHour()
        {
            var cut = CreateTestObject();

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 1, 0, 0);

            cut.First()?.Hour.Should().Be("2022-02-13T10:00:00");
        }

        [Fact]
        public void AddShouldSetProduction()
        {
            var cut = CreateTestObject();

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 1, 0, 0);

            cut.First()?.Production.Should().Be(1);
        }

        [Fact]
        public void AddShouldSetConsumption()
        {
            var cut = CreateTestObject();

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 1, 0);

            cut.First()?.Consumption.Should().Be(1);
        }

        [Fact]
        public void AddShouldSetPrice()
        {
            var cut = CreateTestObject();

            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 1);

            cut.First()?.Price.Should().Be(1);
        }

        [Fact]
        public void AllOnEmptyShouldReturnEmpty()
        {
            var cut = CreateTestObject();

            var res = cut.All();

            res.Should().HaveCount(0);
        }

        [Fact]
        public void AllShouldHaveCount()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);
            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0);

            var res = cut.All();

            res.Should().HaveCount(2);
        }

        [Fact]
        public void GetShouldReturnItem()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);
            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 1, 0, 0);

            var res = cut.Get(DateTime.Parse("2022-02-13T11:00:00"));

            res?.Production.Should().Be(1);
        }

        [Fact]
        public void GetNonExitingItemShouldReturnNull()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);
            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0);

            var res = cut.Get(DateTime.Parse("2022-02-13T12:00:00"));

            res.Should().BeNull();
        }

        [Fact]
        public void GetWithInvalidHourShouldItemReturnCloseHour()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);
            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 1, 0, 0);

            var res = cut.Get(DateTime.Parse("2022-02-13T11:00:01"));

            res?.Production.Should().Be(1);
        }

        [Fact]
        public void FirstOnEmptyShouldBeNull()
        {
            var cut = CreateTestObject();

            var res = cut.First();

            res.Should().BeNull();
        }

        [Fact]
        public void FirstShouldBeFirst()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 1, 0, 0);
            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 0, 0, 0);

            var res = cut.First();

            res?.Production.Should().Be(1);
        }

        [Fact]
        public void LastOnEmptyShouldBeNull()
        {
            var cut = CreateTestObject();

            var res = cut.Last();

            res.Should().BeNull();
        }

        [Fact]
        public void LastShouldBeFirst()
        {
            var cut = CreateTestObject();
            cut.Add(DateTime.Parse("2022-02-13T10:00:00"), 0, 0, 0);
            cut.Add(DateTime.Parse("2022-02-13T11:00:00"), 1, 0, 0);

            var res = cut.Last();

            res?.Production.Should().Be(1);
        }

        [Fact]
        public void ChargeEmptyFlowShouldThrow()
        {
            var cut = CreateTestObject();

            var act = () => cut.Charge(DateTime.Parse("2022-02-13T09:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChargeWhereHourNotFoundFlowShouldThrow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var act = () => cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ChargeShouldChargeBatteryLevelAfterHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 10);

            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 30);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(40);
            res.Skip(2).First().BatteryLevel.Should().Be(40);
        }

        [Fact]
        public void ChargeBeyondSizeShouldChargeBatteryLevelAfterHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 40, inverterLimit: 1000, batteryLevel: 10);

            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 50);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(40);
            res.Skip(2).First().BatteryLevel.Should().Be(40);
        }

        [Fact]
        public void ChargeTwiceBeyondInverterLimitShouldChargeBatteryLevelAfterHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 30, batteryLevel: 10);

            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 20);
            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(40);
            res.Skip(2).First().BatteryLevel.Should().Be(40);
        }

        [Fact]
        public void ChargeShouldReturnActualChargeValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 10);

            var res = cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 30);

            res.Should().Be(30);
        }

        [Fact]
        public void ChargeBeyondSizeShouldReturnActualChargeValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 40, inverterLimit: 1000, batteryLevel: 10);

            var res = cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 50);

            res.Should().Be(30);
        }

        [Fact]
        public void ChargeTwiceBeyondInverterLimitShouldReturnActualChargeValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 30, batteryLevel: 10);

            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 20);
            var res = cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 20);

            res.Should().Be(10);
        }

        [Fact]
        public void ChargeShouldConsiderFutureBatteryLevel()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 50, inverterLimit: 1000, batteryLevel: 0);

            cut.Charge(DateTime.Parse("2022-02-13T11:00:00"), 20).Should().Be(20);
            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 20).Should().Be(20);
            cut.Charge(DateTime.Parse("2022-02-13T09:00:00"), 20).Should().Be(10);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(10);
            res.Skip(1).First().BatteryLevel.Should().Be(30);
            res.Skip(2).First().BatteryLevel.Should().Be(50);
        }

        [Fact]
        public void ChargeShouldChangeCharge()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 0);

            cut.Charge(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();
            res.Skip(0).First().Charge.Should().Be(0);
            res.Skip(1).First().Charge.Should().Be(20);
            res.Skip(2).First().Charge.Should().Be(0);
        }

        [Fact]
        public void ChargeWithItemShouldChangeCharge()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 0);
            var item = cut.Get(DateTime.Parse("2022-02-13T09:00:00")) ?? throw new Exception();

            cut.Charge(item, 20);

            cut.First()?.Charge.Should().Be(20);
        }

        [Fact]
        public void ChargeWithItemWithoutValueShouldChangeCharge()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(20, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 0);
            var item = cut.Get(DateTime.Parse("2022-02-13T09:00:00")) ?? throw new Exception();

            cut.Charge(item);

            cut.First()?.Charge.Should().Be(20);
        }

        [Fact]
        public void ChargeWithNegativeShouldThrow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 0);

            var act = new Action(() => cut.Charge(DateTime.Parse("2022-02-13T09:00:00"), -20));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void DischargeEmptyFlowShouldThrow()
        {
            var cut = CreateTestObject();

            var act = () => cut.Discharge(DateTime.Parse("2022-02-13T09:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void DischargeWhereHourNotFoundFlowShouldThrow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var act = () => cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 1);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void DischargeShouldChargeBatteryLevelAfterHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 1000);

            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 30);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(1000);
            res.Skip(1).First().BatteryLevel.Should().Be(970);
            res.Skip(2).First().BatteryLevel.Should().Be(970);
        }

        [Fact]
        public void DischargeBeyondZeroShouldChangeBatteryLevelAfterHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 40, inverterLimit: 1000, batteryLevel: 30);

            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 50);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(30);
            res.Skip(1).First().BatteryLevel.Should().Be(0);
            res.Skip(2).First().BatteryLevel.Should().Be(0);
        }

        [Fact]
        public void DischargeTwiceBeyondInverterLimitShouldChargeBatteryLevelAfterHour()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 30, batteryLevel: 1000);

            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20).Should().Be(20);
            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20).Should().Be(10);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(1000);
            res.Skip(1).First().BatteryLevel.Should().Be(970);
            res.Skip(2).First().BatteryLevel.Should().Be(970);
        }

        [Fact]
        public void DischargeShouldReturnActualChargeValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 1000);

            var res = cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 30);

            res.Should().Be(30);
        }

        [Fact]
        public void DischargeBeyondZeroShouldReturnActualDishargeValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 30);

            var res = cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 50);

            res.Should().Be(30);
        }

        [Fact]
        public void DischargeTwiceBeyondInverterLimitShouldReturnActualDischargeValue()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 30, batteryLevel: 1000);

            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20);
            var res = cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20);

            res.Should().Be(10);
        }

        [Fact]
        public void DischargeShouldConsiderFutureBatteryLevel()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 100, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 50, inverterLimit: 1000, batteryLevel: 50);

            cut.Discharge(DateTime.Parse("2022-02-13T11:00:00"), 20).Should().Be(20);
            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20).Should().Be(20);
            cut.Discharge(DateTime.Parse("2022-02-13T09:00:00"), 20).Should().Be(10);

            var res = cut.All();
            res.Skip(0).First().BatteryLevel.Should().Be(40);
            res.Skip(1).First().BatteryLevel.Should().Be(20);
            res.Skip(2).First().BatteryLevel.Should().Be(00);
        }

        [Fact]
        public void DischargeShouldChangeCharge()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);

            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();
            res.Skip(0).First().Charge.Should().Be(0);
            res.Skip(1).First().Charge.Should().Be(-20);
            res.Skip(2).First().Charge.Should().Be(0);
        }

        [Fact]
        public void DischargeBeyondZeroShouldChangeChargeWithLimitation()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 10);

            cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 20);

            var res = cut.All();
            res.Skip(0).First().Charge.Should().Be(0);
            res.Skip(1).First().Charge.Should().Be(-10);
            res.Skip(2).First().Charge.Should().Be(0);
        }

        [Fact]
        public void DischargeMoreThanNeedShouldDischargeNeed()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batteryLevel: 1000);

            var res = cut.Discharge(DateTime.Parse("2022-02-13T10:00:00"), 150);

            res.Should().Be(100);
        }

        [Fact]
        public void DischargeWithItemShouldChangeCharge()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 1000, 0);
            var cut = CreateTestObject(testData: testData, batteryLevel: 1000);
            var item = cut.Get(DateTime.Parse("2022-02-13T09:00:00")) ?? throw new Exception();

            cut.Discharge(item, 20);

            cut.First()?.Charge.Should().Be(-20);
        }

        [Fact]
        public void DischargeWithItemWithoutValueShouldChangeCharge()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 20, 0);
            var cut = CreateTestObject(testData: testData, batteryLevel: 1000);
            var item = cut.Get(DateTime.Parse("2022-02-13T09:00:00")) ?? throw new Exception();

            cut.Discharge(item);

            cut.First()?.Charge.Should().Be(-20);
        }

        [Fact]
        public void DischargeWithNegativeShouldThrow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData);

            var act = new Action(() => cut.Discharge(DateTime.Parse("2022-02-13T09:00:00"), -20));

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MoveNegativNumberShouldThrow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);
            var source = cut.Get(DateTime.Parse("2022-02-13T10:00:00")) ?? throw new Exception();
            var target = cut.Get(DateTime.Parse("2022-02-13T11:00:00")) ?? throw new Exception();

            var act = new Action(() => cut.Move(source, target, -10));

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MoveWithSameItemOrderShouldThrow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);
            var source = cut.Get(DateTime.Parse("2022-02-13T10:00:00")) ?? throw new Exception();
            var target = cut.Get(DateTime.Parse("2022-02-13T10:00:00")) ?? throw new Exception();

            var act = new Action(() => cut.Move(source, target, 10));

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MoveForwardPowerBeyondSizeShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 80, batteryLevel: 50);

            var res = cut.Move(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 40);

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
        public void MoveForwardBeyondInverterLimitShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 100, inverterLimit: 20, batteryLevel: 50);

            var res = cut.Move(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 40);

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
        public void MoveForwardBeyondInverterOnDischargeLimitShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 100, inverterLimit: 40, batteryLevel: 50);
            cut.Discharge(DateTime.Parse("2022-02-13T11:00:00"), 15);

            var res = cut.Move(DateTime.Parse("2022-02-13T10:00:00"), DateTime.Parse("2022-02-13T11:00:00"), 30);

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
        public void MoveForwardShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);
            var source = cut.Get(DateTime.Parse("2022-02-13T10:00:00")) ?? throw new Exception();
            var target = cut.Get(DateTime.Parse("2022-02-13T11:00:00")) ?? throw new Exception();

            var res = cut.Move(source, target, 10);

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
        public void MoveForwardWithoutValueShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            var cut = CreateTestObject(testData: testData, batteryLevel: 1000);
            var source = cut.Get(DateTime.Parse("2022-02-13T10:00:00")) ?? throw new Exception();
            var target = cut.Get(DateTime.Parse("2022-02-13T11:00:00")) ?? throw new Exception();

            var res = cut.Move(source, target);

            res.Should().Be(100);
        }

        [Fact]
        public void MoveBackwardShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);

            var res = cut.Move(DateTime.Parse("2022-02-13T11:00:00"), DateTime.Parse("2022-02-13T10:00:00"), 10);

            res.Should().Be(10);
            var flow = cut.All();
            flow.Skip(0).First().BatteryLevel.Should().Be(50);
            flow.Skip(1).First().BatteryLevel.Should().Be(40);
            flow.Skip(2).First().BatteryLevel.Should().Be(50);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(-10);
            flow.Skip(2).First().Charge.Should().Be(10);
        }

        [Fact]
        public void MoveBackwardShouldReturnFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(0, 0, 0);
            testData.Add(0, 100, 0);
            testData.Add(0, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);

            var res = cut.Move(DateTime.Parse("2022-02-13T11:00:00"), DateTime.Parse("2022-02-13T10:00:00"));

            res.Should().Be(10);
            var flow = cut.All();
            flow.Skip(0).First().BatteryLevel.Should().Be(50);
            flow.Skip(1).First().BatteryLevel.Should().Be(40);
            flow.Skip(2).First().BatteryLevel.Should().Be(50);
            flow.Skip(0).First().Charge.Should().Be(0);
            flow.Skip(1).First().Charge.Should().Be(-10);
            flow.Skip(2).First().Charge.Should().Be(10);
        }

        [Fact]
        public void ForeachShouldListFlow()
        {
            var testData = new TestPowerFlow("2022-02-13T09:00:00");
            testData.Add(1, 0, 0);
            testData.Add(2, 0, 0);
            testData.Add(3, 0, 0);
            var cut = CreateTestObject(testData: testData, batterySize: 1000, inverterLimit: 1000, batteryLevel: 50);
            var i = 1;

            foreach(var item in cut)
            {
                item.Production.Should().Be(i++);
            }
        }

        private IPowerFlow CreateTestObject(int batterySize = 10000, int inverterLimit = 3300, int batteryLevel = 0, TestPowerFlow? testData = null)
        {
            _houseBatteryService.GetBatterySize().Returns(batterySize);
            _houseBatteryService.InverterLimit().Returns(inverterLimit);
            _houseBatteryService.GetBatteryCurrent().Returns(batteryLevel);

            return _powerFlowFactory.Instance(testData?.Hour ?? DateTime.Parse("2022-02-13T10:00:00"), testData?.Productions ?? new List<ProductionItem>(), testData?.Consumptions ?? new List<ConsumptionItem>(), testData?.Prices ?? new List<PriceItem>());
        }
    }
}
