using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen.Dtos;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.HouseBattery.Client.Sonnen.IntegrationTests
{
    public class SonnenBatteryClientTests
    {
        private readonly SonnenBatteryClientOptions _options = new() { 
            Ip = "192.168.0.222", 
            ApiToken = "0ca39846-405a-4c8f-b48c-41f46fe17be1" 
        };

        [Fact]
        public void GetBatteryLevelShouldReturnValidLevel()
        {
            var cut = CreateTestObject();

            var res = cut.GetBatteryLevel();

            res.Should().BeInRange(0, 100);
        }

        [Fact]
        public void GetConsumptionShouldReturnValidLevel()
        {
            var cut = CreateTestObject();

            var res = cut.GetConsumption();

            res.Should().BeInRange(0, 100000);
        }

        [Fact]
        public void GetProductionShouldReturnValidLevel()
        {
            var cut = CreateTestObject();

            var res = cut.GetProduction();

            res.Should().BeInRange(0, 100000);
        }

        [Fact]
        public void GetBatteryCapacityShouldReturnValidLevel()
        {
            var cut = CreateTestObject();

            var res = cut.GetBatteryCapacity();

            res.Should().BeInRange(0, 100000);
        }

        [Fact]
        public void GetBatterySizeShouldReturnValidLevel()
        {
            var cut = CreateTestObject();

            var res = cut.GetBatterySize();

            res.Should().BeInRange(1, 100000);
        }

        [Fact]
        public void CanGetOperatingMode()
        {
            var cut = CreateTestObject();

            var act = () => cut.GetOperatingMode();

            act.Should().NotThrow();
        }

        [Fact]
        public void CanGetTimeOfUseSchedule()
        {
            var cut = CreateTestObject();

            var act = () => cut.GetSchedule();

            act.Should().NotThrow();
        }

        [Fact]
        public void CanChangeOperatingMode()
        {
            var cut = CreateTestObject();

            var initial = cut.GetOperatingMode();

            cut.SetOperatingMode(OperatingMode.SelfConsumption);
            cut.GetOperatingMode().Should().Be(OperatingMode.SelfConsumption);

            cut.SetOperatingMode(OperatingMode.Manual);
            cut.GetOperatingMode().Should().Be(OperatingMode.Manual);

            cut.SetOperatingMode(initial);
        }

        [Fact]
        public void CanChangeTimeOfUseSchedule()
        {
            var cut = CreateTestObject();

            var initial = cut.GetSchedule();

            var schedule = new List<TimeOfUseEvent>()
            {
                new TimeOfUseEvent() { Start = "03:00", End = "04:00", Watt = 3000},
                new TimeOfUseEvent() { Start = "23:00", End = "00:00", Watt = 2000}
            };

            cut.SetSchedule(schedule);
            cut.GetSchedule().Should().HaveCount(2);

            cut.SetSchedule(Enumerable.Empty<TimeOfUseEvent>());
            cut.GetSchedule().Should().HaveCount(0);

            cut.SetSchedule(initial);
        }

        private ISonnenBatteryClient CreateTestObject()
        {
            return new SonnenBatteryClient(Options.Create(_options), new RestClientFactory(Substitute.For<ILoggerFactory>()));
        }
    }
}