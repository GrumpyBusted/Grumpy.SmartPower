using FluentAssertions;
using Grumpy.SolarInformation.Interface;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests
{
    public class SolarServiceTests
    {
        [Fact]
        public void SunlightShouldReturnFromSolarInformation()
        {
            var options = new SolarServiceOptions();
            var solarInformation = Substitute.For<ISolarInformation>();
            solarInformation.SunlightPerHour(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateTime>()).Returns(TimeSpan.FromMinutes(33));

            var cut = new SolarService(Options.Create(options), solarInformation);

            var res = cut.Sunlight(DateTime.Parse("2022-02-13T07:00:00"));

            res.Should().Be(TimeSpan.FromMinutes(33));
        }

        [Fact]
        public void AltitudeShouldReturnFromSolarInformation()
        {
            var options = new SolarServiceOptions();
            var solarInformation = Substitute.For<ISolarInformation>();
            solarInformation.AltitudePerHour(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateTime>()).Returns(10);

            var cut = new SolarService(Options.Create(options), solarInformation);

            var res = cut.Altitude(DateTime.Parse("2022-02-13T07:00:00"));

            res.Should().Be(10);
        }

        [Fact]
        public void DirectionShouldReturnFromSolarInformation()
        {
            var options = new SolarServiceOptions();
            var solarInformation = Substitute.For<ISolarInformation>();
            solarInformation.DirectionPerHour(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateTime>()).Returns(10);

            var cut = new SolarService(Options.Create(options), solarInformation);

            var res = cut.Direction(DateTime.Parse("2022-02-13T07:00:00"));

            res.Should().Be(10);
        }
    }
}