using FluentAssertions;
using Grumpy.TestTools.Extensions;
using System;
using Grumpy.SolarInformation.Interface;
using Xunit;

namespace Grumpy.SolarInformation.UnitTests;

public class SolarInformationTests
{
    [Fact]
    public void SunriseInSydneyShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunrise(-33.856837, 151.215066, new DateOnly(2020, 7, 21), TimeSpan.FromHours(10));

        res.Should().Be("2020-07-21T06:55:14");
    }

    [Fact]
    public void SunriseInJanuaryShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunrise(55.5755, 12.2931, new DateOnly(2022, 1, 1));

        res.Should().Be("2022-01-01T08:39:21");
    }

    [Fact]
    public void SunriseBeforeDayLightSavingShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunrise(55.5755, 12.2931, new DateOnly(2022, 3, 26));

        res.Should().Be("2022-03-26T05:58:29");
    }

    [Fact]
    public void SunriseAfterDayLightSavingShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunrise(55.5755, 12.2931, new DateOnly(2022, 3, 27));

        res.Should().Be("2022-03-27T06:55:59");
    }

    [Fact]
    public void SunriseInJuneShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunrise(55.5755, 12.2931, new DateOnly(2022, 6, 24));

        res.Should().Be("2022-06-24T04:27:58");
    }

    [Fact]
    public void SunsetInSydneyShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunset(-33.856837, 151.215066, new DateOnly(2020, 7, 21), TimeSpan.FromHours(10));

        res.Should().Be("2020-07-21T17:07:52");
    }

    [Fact]
    public void SunsetInJanuaryShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunset(55.5755, 12.2931, new DateOnly(2022, 1, 1));

        res.Should().Be("2022-01-01T15:48:54");
    }

    [Fact]
    public void SunsetInJuneShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Sunset(55.5755, 12.2931, new DateOnly(2022, 6, 24));

        res.Should().Be("2022-06-24T21:58:20");
    }

    [Fact]
    public void SunlightAtNightShouldBeZero()
    {
        var cut = CreateTestObject();

        var res = cut.Sunlight(55.5755, 12.2931, DateTime.Parse("2022-06-24T23:00:00"), DateTime.Parse("2022-06-24T23:05:00"));

        res.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void SunlightAtNoonShouldBeFullTime()
    {
        var cut = CreateTestObject();

        var res = cut.Sunlight(55.5755, 12.2931, DateTime.Parse("2022-06-24T12:00:00"), DateTime.Parse("2022-06-24T12:05:00"));

        res.TotalMinutes.Should().Be(5);
    }

    [Fact]
    public void SunlightAtSunriseShouldBeFromSunrise()
    {
        // Sunrise is "2022-06-24T04:27:58"
        var cut = CreateTestObject();

        var res = cut.Sunlight(55.5755, 12.2931, DateTime.Parse("2022-06-24T04:25:58"), DateTime.Parse("2022-06-24T04:32:58"));

        res.TotalMinutes.Should().BeApproximately(5, 0.1);
    }

    [Fact]
    public void SunlightAtSunsetShouldBeToSunset()
    {
        // Sunset is "2022-06-24T21:58:20"
        var cut = CreateTestObject();

        var res = cut.Sunlight(55.5755, 12.2931, DateTime.Parse("2022-06-24T21:53:20"), DateTime.Parse("2022-06-24T22:00:20"));

        res.TotalMinutes.Should().BeApproximately(5, 0.1);
    }

    [Fact]
    public void SunlightPerHourShouldBeFromSunrise()
    {
        var cut = CreateTestObject();

        var res = cut.SunlightPerHour(55.5755, 12.2931, DateTime.Parse("2022-06-24T12:00:00"));

        res.TotalMinutes.Should().Be(60);
    }

    [Fact]
    public void SunlightPerHourAtSunriseShouldBeFromSunrise()
    {
        // Sunrise is "2022-06-24T04:27:58"
        var cut = CreateTestObject();

        var res = cut.SunlightPerHour(55.5755, 12.2931, DateTime.Parse("2022-06-24T04:00:00"));

        res.TotalMinutes.Should().BeApproximately(32, 1);
    }

    [Fact]
    public void SunlightPerHourAtSunsetShouldBeToSunset()
    {
        // Sunset is "2022-06-24T21:58:20"
        var cut = CreateTestObject();

        var res = cut.SunlightPerHour(55.5755, 12.2931, DateTime.Parse("2022-06-24T21:00:00"));

        res.TotalMinutes.Should().BeApproximately(58, 1);
    }

    [Fact]
    public void SunlightPerHourAtMidnightShouldBeZero()
    {
        // Sunset is "2022-06-24T21:58:20"
        var cut = CreateTestObject();

        var res = cut.SunlightPerHour(55.5755, 12.2931, DateTime.Parse("2022-06-24T23:00:00"));

        res.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void AltitudeAtMidSummerShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Altitude(55.5755, 12.2931, DateTime.Parse("2022-06-22T13:13:00"));

        res.Should().BeApproximately(57.8, 0.1);
    }

    [Fact]
    public void AltitudeAtMidWinterShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Altitude(55.5755, 12.2931, DateTime.Parse("2022-12-22T12:13:00"));

        res.Should().BeApproximately(10.9, 0.1);
    }

    [Fact]
    public void AltitudeInMorningShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Altitude(55.5755, 12.2931, DateTime.Parse("2022-12-22T09:00:00"));

        res.Should().BeApproximately(1.3, 0.1);
    }

    [Fact]
    public void AverageAltitudeInMorningShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Altitude(55.5755, 12.2931, DateTime.Parse("2022-12-22T09:00:00"), DateTime.Parse("2022-12-22T10:00:00"));

        res.Should().BeApproximately(3.9, 0.1);
    }

    [Fact]
    public void AverageAltitudePerHourShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.AltitudePerHour(55.5755, 12.2931, DateTime.Parse("2022-12-22T09:00:00"));

        res.Should().BeApproximately(3.9, 0.1);
    }

    [Fact]
    public void DirectionAtNoonShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Direction(55.5755, 12.2931, DateTime.Parse("2022-12-22T12:00:00"));

        res.Should().BeApproximately(177.9, 0.1);
    }

    [Fact]
    public void DirectionAtNoonInSummerShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Direction(55.5755, 12.2931, DateTime.Parse("2022-06-22T12:00:00"));

        res.Should().BeApproximately(149.9, 0.1);
    }

    [Fact]
    public void DirectionAtSunriseInSummerShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Direction(55.5755, 12.2931, DateTime.Parse("2022-06-22T05:00:00"));

        res.Should().BeApproximately(50.2, 0.1);
    }

    [Fact]
    public void AverageDirectionInMorningInSummerShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.Direction(55.5755, 12.2931, DateTime.Parse("2022-06-22T05:00:00"), DateTime.Parse("2022-06-22T06:00:00"));

        res.Should().BeApproximately(56.2, 0.1);
    }

    [Fact]
    public void AverageDirectionPerHourInSummerShouldBeCorrect()
    {
        var cut = CreateTestObject();

        var res = cut.DirectionPerHour(55.5755, 12.2931, DateTime.Parse("2022-06-22T05:00:00"));

        res.Should().BeApproximately(56.2, 0.1);
    }

    private static ISolarInformation CreateTestObject()
    {
        return new SolarInformation();
    }
}