using FluentAssertions;
using Grumpy.Common;
using Grumpy.Common.Interface;
using Grumpy.SmartPower.Core.Model;
using Grumpy.TestTools.Extensions;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests
{
    public class WeatherServiceTests
    {
        [Fact]
        public void GetSunInformationShouldReturnInformationFromClient()
        {
            var openWeatherMapClient = Substitute.For<IOpenWeatherMapClient>();
            openWeatherMapClient.GetSunInformation().Returns(new SunInformation
            {
                Sunrise = DateTime.Parse("2022-02-13T07:00:00"),
                Sunset = DateTime.Parse("2022-02-13T17:00:00")
            });
            var visualCrossingWeatherClient = Substitute.For<IVisualCrossingWeatherClient>();

            var cut = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient, new DateTimeProvider());

            var res = cut.GetSunInformation();

            res.Sunrise.Should().Be("2022-02-13T07:00:00");
            res.Sunset.Should().Be("2022-02-13T17:00:00");
        }

        [Fact]
        public void GetForecastShouldReturnList()
        {
            var list = new List<WeatherItem>
        {
            new()
            {
                Hour = DateTime.Parse("2022-01-02T00:00:00")
            },
            new()
            {
                Hour = DateTime.Parse("2022-01-03T00:00:00")
            }
        };

            var openWeatherMapClient = Substitute.For<IOpenWeatherMapClient>();
            openWeatherMapClient.GetForecast().Returns(list);

            var visualCrossingWeatherClient = Substitute.For<IVisualCrossingWeatherClient>();
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.Now.Returns(DateTime.Parse("2022-01-01T00:00:00"));

            var cut = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient, dateTimeProvider);

            var res = cut.GetForecast(DateTime.Parse("2022-01-02T00:00:00"), DateTime.Parse("2022-01-02T23:59:59"));

            res.Should().HaveCount(1);
        }

        [Fact]
        public void GetHistoryShouldReturnList()
        {
            var list = new List<WeatherItem>
        {
            new()
            {
                Hour = DateTime.Parse("2022-01-03T00:00:00")
            },
            new()
            {
                Hour = DateTime.Parse("2022-01-03T12:00:00")
            },
            new()
            {
                Hour = DateTime.Parse("2022-01-03T23:00:00")
            }
        };

            var openWeatherMapClient = Substitute.For<IOpenWeatherMapClient>();

            var visualCrossingWeatherClient = Substitute.For<IVisualCrossingWeatherClient>();
            visualCrossingWeatherClient.Get(Arg.Any<DateOnly>()).Returns(list);

            var cut = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient, new DateTimeProvider());

            var res = cut.GetHistory(DateTime.Parse("2022-01-03T11:00:00"), DateTime.Parse("2022-01-03T13:00:00")).ToList();

            res.Should().HaveCount(1);
            res.First().Hour.Should().Be(DateTime.Parse("2022-01-03T12:00:00"));
        }
    }
}