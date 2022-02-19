using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using Grumpy.SmartPower.Infrastructure;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.SmartPower.Core.IntegrationTests
{
    public class ProductionServiceTests
    {

        [Fact]
        public void FullDayWithNotCloudsShouldReturnList()
        {
            var solarServiceOptions = new SolarServiceOptions
            {
                Latitude = 55.5755,
                Longitude = 12.2931
            };
            var solarService = new SolarService(Options.Create(solarServiceOptions), new SolarInformation.SolarInformation());

            var weatherService = Substitute.For<IWeatherService>();
            var forecast = new List<WeatherItem>();
            while (forecast.Count < 24)
            {
                forecast.Add(new WeatherItem
                {
                    Hour = DateTime.Parse("2022-02-13T00:00:00").AddHours(forecast.Count),
                    CloudCover = 0
                });
            }
            weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(forecast);

            var productionServiceOptions = new ProductionServiceOptions
            {
                Angle = 20,
                Direction = 112,
                Capacity = 9000
            };
            var cut = new ProductionService(Options.Create(productionServiceOptions), solarService, weatherService);

            var res = cut.Forecast(DateTime.Parse("2022-02-13T00:00:00"), DateTime.Parse("2022-02-13T23:59:59")).ToList();

            res.Should().HaveCount(24);
            res.First().WattPerHour.Should().Be(0);
            res.First(i => i.Hour.Hour == 11).WattPerHour.Should().BeCloseTo(3500, 500);
        }
    }
}