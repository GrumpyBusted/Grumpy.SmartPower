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

namespace Grumpy.SmartPower.Core.UnitTests;

public class ProductionServiceTests
{
    [Fact]
    public void PerfectConditionShouldReturnPanelCapacity()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 112, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(9000);
    }

    [Fact]
    public void LittleCloudedShouldReturnPanelCapacity()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 112, 1, 20);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(7200);
    }

    [Fact]
    public void FullCloudShouldReturnZero()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 112, 1, 100);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void SunlightHalfTheHourShouldReturnHalf()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 112, 0.5, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(4500);
    }

    [Fact]
    public void NoSunlightTheHourShouldReturnZero()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 112, 0, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void East10DegreesShouldAlmostFull()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 102, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(9000, 400);
    }

    [Fact]
    public void East80DegreesShouldLittle()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 32, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(0, 400);
    }

    [Fact]
    public void East90DegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 22, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void East100DegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 12, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void EastPassedZeroDegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 352, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void West10DegreesWestShouldAlmostFull()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 122, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(9000, 400);
    }

    [Fact]
    public void West80DegreesShouldLittle()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 192, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(0, 400);
    }

    [Fact]
    public void West90DegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 202, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void West100DegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 212, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void WestALotDegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, 70, 350, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void BeforeSunriseDegreesShouldNothing()
    {
        var cut = CreateTestObject(20, 112, 9000, -1, 112, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().Be(0);
    }

    [Fact]
    public void SunriseDegreesShouldLittle()
    {
        var cut = CreateTestObject(20, 112, 9000, 0, 112, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(0, 400);
    }

    [Fact]
    public void WinterMiddayDegreesShouldSome()
    {
        var cut = CreateTestObject(20, 112, 9000, 10, 112, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(2000, 400);
    }

    [Fact]
    public void SummerMiddayShouldBeAlmostFull()
    {
        var cut = CreateTestObject(20, 112, 9000, 60, 112, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T12:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(9000, 400);
    }

    [Fact]
    public void AcceptCase1ShouldMatch()
    {
        // Actual readings
        var cut = CreateTestObject(23, 112, 9000, 12, 160, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-01-17T11:00:00"), DateTime.Parse("2022-01-17T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(2900, 400);
    }

    [Fact]
    public void AcceptCase2ShouldMatch()
    {
        // Actual readings
        var cut = CreateTestObject(23, 112, 9000, 14, 160, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-01-30T11:00:00"), DateTime.Parse("2022-01-30T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(3900, 400);
    }

    [Fact]
    public void AcceptCase3ShouldMatch()
    {
        // Actual readings
        var cut = CreateTestObject(23, 112, 9000, 18, 160, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-02-13T11:00:00"), DateTime.Parse("2022-02-13T12:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(4900, 400);
    }

    [Fact]
    public void AcceptCase4ShouldMatch()
    {
        // Estimated readings
        var cut = CreateTestObject(23, 112, 9000, 55, 150, 1, 0);

        var res = cut.Predict(DateTime.Parse("2022-06-22T12:00:00"), DateTime.Parse("2022-06-22T13:00:00"));

        res.First().WattPerHour.Should().BeCloseTo(7000, 200);
    }


    [Fact]
    public void GetDataShouldReturnData()
    {
        var hour = DateTime.Parse("2022-02-21T09:00:00");

        var weatherItem = new WeatherItem
        {
            Hour = hour,
            Temperature = 1,
            CloudCover = 2,
            WindSpeed = 3
        };

        var cut = CreateTestObject(23, 112, 9000, 55, 150, 0.5, 0);

        var res = cut.GetData(weatherItem);

        res.Hour.Should().Be(DateTime.Parse("2022-02-21T09:00:00"));
        res.Weather.Temperature.Should().Be(1);
        res.Weather.CloudCover.Should().Be(2);
        res.Weather.WindSpeed.Should().Be(3);
        res.Sun.Sunlight.Should().Be(TimeSpan.FromHours(0.5));
        res.Sun.Altitude.Should().Be(55);
        res.Sun.Direction.Should().Be(150);
        res.Sun.HorizontalAngle.Should().Be(52);
        res.Sun.VerticalAngle.Should().Be(78);
        res.Calculated.Should().Be(3430);
    }


    private static ProductionService CreateTestObject(int panelAngle, int panelDirection, int panelCapacity, double sunAltitude, double sunDirection, double sunlightHours, int cloudCover)
    {
        var options = new ProductionServiceOptions
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
        weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>
        {
            new()
            {
                Hour = DateTime.Parse("2022-02-13T12:00:00"),
                CloudCover = cloudCover
            }
        });
        var predictProductionService = Substitute.For<IPredictProductionService>();

        return new ProductionService(Options.Create(options), solarService, weatherService, predictProductionService);
    }
}