using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.SmartPower.Core.UnitTests;

public class ConsumptionServiceTests
{
    private readonly IPowerMeterService _powerMeterService = Substitute.For<IPowerMeterService>();
    private readonly IWeatherService _weatherService = Substitute.For<IWeatherService>();
    private readonly IPredictConsumptionService _predictConsumptionService = Substitute.For<IPredictConsumptionService>();
    private readonly IRealTimeReadingRepository _realTimeReadingRepository = Substitute.For<IRealTimeReadingRepository>();

    [Fact]
    public void PredictWithResultFromPredictionServiceShouldUseValue()
    {
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>
        {
            new()
            {
                Hour = DateTime.Parse("2022-02-21T09:00:00")
            }
        });
        _predictConsumptionService.Predict(Arg.Any<ConsumptionData>()).Returns(1);

        var cut = CreateTestObject();

        var res = cut.Predict(DateTime.Parse("2022-02-21T09:00:00"), DateTime.Parse("2022-02-21T10:00:00"));

        res.First().WattPerHour.Should().Be(1);
    }

    [Fact]
    public void PredictNoAnswerFromPredictionServiceShouldLastWeekConsumption()
    {
        var testDate = DateTime.Parse("2022-02-21T09:00:00");
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>
        {
            new()
            {
                Hour = testDate
            }
        });
        _predictConsumptionService.Predict(Arg.Any<ConsumptionData>()).Returns(_ => null);
        _realTimeReadingRepository.GetConsumption(Arg.Is(testDate.AddDays(-7))).Returns(1);

        var cut = CreateTestObject();

        var res = cut.Predict(testDate, testDate.AddMinutes(59));

        res.First().WattPerHour.Should().Be(1);
    }

    [Fact]
    public void PredictNoAnswerFromPredictionServiceAndNothingFromRepositoryShouldLastWeekConsumptionFromPowerMeter()
    {
        var testDate = DateTime.Parse("2022-02-21T09:00:00");
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>
        {
            new()
            {
                Hour = testDate
            }
        });
        _predictConsumptionService.Predict(Arg.Any<ConsumptionData>()).Returns(_ => null);
        _realTimeReadingRepository.GetConsumption(Arg.Is(testDate.AddDays(-7))).Returns(_ => null);
        _powerMeterService.GetWattPerHour(Arg.Is(testDate.AddDays(-7))).Returns(1);

        var cut = CreateTestObject();

        var res = cut.Predict(testDate, testDate.AddMinutes(59));

        res.First().WattPerHour.Should().Be(1);
    }

    [Fact]
    public void TwoHoursInForecastShouldReturnListOfTwo()
    {
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>
        {
            new()
            {
                Hour = DateTime.Parse("2022-02-21T09:00:00")
            },
            new()
            {
                Hour = DateTime.Parse("2022-02-21T10:00:00")
            }
        });

        var cut = CreateTestObject();

        var res = cut.Predict(DateTime.Parse("2022-02-21T09:00:00"), DateTime.Parse("2022-02-21T11:00:00"));

        res.Should().HaveCount(2);
    }

    [Fact]
    public void GetDataShouldReturnData()
    {
        var hour = DateTime.Parse("2022-02-21T09:00:00");
        _weatherService.GetHistory(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>
        {
            new()
            {
                Hour = hour.AddDays(-1),
                Temperature = 4,
                CloudCover = 5,
                WindSpeed = 6
            },
            new()
            {
                Hour = hour.AddDays(-7),
                Temperature = 7,
                CloudCover = 8,
                WindSpeed = 9
            },
            new()
            {
                Hour = hour.AddDays(-8),
                Temperature = 10,
                CloudCover = 11,
                WindSpeed = 12
            }
        });
        _realTimeReadingRepository.GetConsumption(Arg.Is(hour.AddDays(-1))).Returns(13);
        _realTimeReadingRepository.GetConsumption(Arg.Is(hour.AddDays(-7))).Returns(14);
        _realTimeReadingRepository.GetConsumption(Arg.Is(hour.AddDays(-8))).Returns(15);

        var cut = CreateTestObject();

        var weatherItem = new WeatherItem
        {
            Hour = hour,
            Temperature = 1,
            CloudCover = 2,
            WindSpeed = 3
        };

        var res = cut.GetData(weatherItem);

        res.Hour.Should().Be(DateTime.Parse("2022-02-21T09:00:00"));
        res.Weather.Forecast.Temperature.Should().Be(1);
        res.Weather.Forecast.CloudCover.Should().Be(2);
        res.Weather.Forecast.WindSpeed.Should().Be(3);
        res.Weather.Yesterday.Temperature.Should().Be(4);
        res.Weather.Yesterday.CloudCover.Should().Be(5);
        res.Weather.Yesterday.WindSpeed.Should().Be(6);
        res.Weather.LastWeek.Temperature.Should().Be(7);
        res.Weather.LastWeek.CloudCover.Should().Be(8);
        res.Weather.LastWeek.WindSpeed.Should().Be(9);
        res.Weather.LastWeekFromYesterday.Temperature.Should().Be(10);
        res.Weather.LastWeekFromYesterday.CloudCover.Should().Be(11);
        res.Weather.LastWeekFromYesterday.WindSpeed.Should().Be(12);
        res.Consumption.Yesterday.Should().Be(13);
        res.Consumption.LastWeek.Should().Be(14);
        res.Consumption.LastWeekFromYesterday.Should().Be(15);
        res.Consumption.WeekFactor.Should().BeApproximately(0.867, 0.01);
    }

    private IConsumptionService CreateTestObject()
    {
        return new ConsumptionService(_powerMeterService, _weatherService, _predictConsumptionService, _realTimeReadingRepository);
    }
}