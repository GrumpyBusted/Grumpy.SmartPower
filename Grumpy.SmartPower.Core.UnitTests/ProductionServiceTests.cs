using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests
{
    public class ProductionServiceTests
    {
        [Fact]
        public void PerfectConditionShouldReturnPanelCapacity()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 112, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(9000);
        }

        [Fact]
        public void LittleCloudedShouldReturnPanelCapacity()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 112, 1, 20);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(7200);
        }

        [Fact]
        public void FullCloudShouldReturnZero()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 112, 1, 100);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void SunlightHalfTheHourShouldReturnHalf()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 112, 0.5, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(4500);
        }

        [Fact]
        public void NoSunlightTheHourShouldReturnZero()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 112, 0, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void East10DegressShouldAlmostFull()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 102, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(9000, 400);
        }

        [Fact]
        public void East80DegressShouldLittle()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 32, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(0, 400);
        }

        [Fact]
        public void East90DegressShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 22, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void East100DegressShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 12, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void EastPassedZeroDegressShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 352, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void West10DegressWestShouldAlmostFull()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 122, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(9000, 400);
        }

        [Fact]
        public void West80DegressShouldLittle()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 192, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(0, 400);
        }

        [Fact]
        public void West90DegreesShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 202, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void West100DegreesShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 212, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void WestAlotDegreesShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, 70, 350, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void BerforeSunriseDegreesShouldNothing()
        {
            var cut = CreateTestObject(20, 112, 9000, -1, 112, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().Be(0);
        }

        [Fact]
        public void SunriseDegreesShouldLittle()
        {
            var cut = CreateTestObject(20, 112, 9000, 0, 112, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(0, 400);
        }

        [Fact]
        public void WinterMiddayDegreesShouldSome()
        {
            var cut = CreateTestObject(20, 112, 9000, 10, 112, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(2000, 400);
        }

        [Fact]
        public void SummerMiddayShouldBeAlmostFull()
        {
            var cut = CreateTestObject(20, 112, 9000, 60, 112, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(9000, 400);
        }

        [Fact]
        public void AcceptCase1ShouldMatch()
        {
            // Actual readings
            var cut = CreateTestObject(23, 112, 9000, 12, 160, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-01-17T11:00:00"), DateTime.Parse("2022-01-17T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(2900, 400);
        }

        [Fact]
        public void AcceptCase2ShouldMatch()
        {
            // Actual readings
            var cut = CreateTestObject(23, 112, 9000, 14, 160, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-01-30T11:00:00"), DateTime.Parse("2022-01-30T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(3900, 400);
        }

        [Fact]
        public void AcceptCase3ShouldMatch()
        {
            // Actual readings
            var cut = CreateTestObject(23, 112, 9000, 18, 160, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T11:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

            res.First().WattHour.Should().BeCloseTo(4900, 400);
        }

        [Fact]
        public void AcceptCase4ShouldMatch()
        {
            // Estimated readings
            var cut = CreateTestObject(23, 112, 9000, 55, 150, 1, 0);

            var res = cut.Forecast(DateTime.Parse("2022-06-22T12:00:00"), DateTime.Parse("2022-06-22T13:00:00"));

            res.First().WattHour.Should().BeCloseTo(7000, 200);
        }

        private static ProductionService CreateTestObject(int panelAngle, int panelDirection, int panelCapacity, double sunAltitude, double sunDirection, double sunlightHours, int cloudCover) 
        {
            var options = new ProductionServiceOptions()
            {
                Angle = panelAngle,
                Direction = panelDirection,
                Capacity = panelCapacity
            };

            var solarService = Substitute.For<ISolarService>();
            solarService.Altitude(Arg.Any<DateTime>()).Returns(sunAltitude);
            solarService.Direction(Arg.Any<DateTime>()).Returns(sunDirection);
            solarService.Sunlight(Arg.Any<DateTime>()).Returns(TimeSpan.FromHours(sunlightHours));

            var weatherService = Substitute.For<IWeatherService>();
            weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>()
            {
                new WeatherItem()
                {
                    Hour = DateTime.Parse("2022-02-13T12:00:00"),
                    CloudCover = cloudCover
                }
            });

            return new ProductionService(Options.Create(options), solarService, weatherService);
        }
    }
}