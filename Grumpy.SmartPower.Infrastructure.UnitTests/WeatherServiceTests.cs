using FluentAssertions;
using Grumpy.SmartPower.Core.Model;
using Grumpy.TestTools.Extensions;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests
{
    public class WeatherServiceTests
    {
        [Fact]
        public void GetSunInformationShouldReturnInformationFromClient()
        {
            var openWeatherMapClient = Substitute.For<IOpenWeatherMapClient>();
            openWeatherMapClient.GetSunInformation().Returns(new SunInformation()
            {
                Sunrise = DateTime.Parse("2022-02-13T07:00:00"),
                Sunset = DateTime.Parse("2022-02-13T17:00:00")
            });
            var visualCrossingWeatherClient = Substitute.For<IVisualCrossingWeatherClient>();

            var cut = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient);

            var res = cut.GetSunInformation();

            res.Sunrise.Should().Be("2022-02-13T07:00:00");
            res.Sunset.Should().Be("2022-02-13T17:00:00");
        }

        [Fact]
        public void GetForecastShouldReturnList()
        {
            var list = new List<WeatherItem>()
            {
                new WeatherItem()
                {
                    Hour = DateTime.Parse("2022-01-02T00:00:00")
                },
                new WeatherItem()
                {
                    Hour = DateTime.Parse("2022-01-03T00:00:00")
                }
            };

            var openWeatherMapClient = Substitute.For<IOpenWeatherMapClient>();
            openWeatherMapClient.GetForecast().Returns(list);

            var visualCrossingWeatherClient = Substitute.For<IVisualCrossingWeatherClient>();

            var cut = new WeatherService(openWeatherMapClient, visualCrossingWeatherClient);

            var res = cut.GetForecast(DateTime.Parse("2022-01-02T00:00:00"), DateTime.Parse("2022-01-02T23:59:59"));

            res.Should().HaveCount(1);
        }
    }
}