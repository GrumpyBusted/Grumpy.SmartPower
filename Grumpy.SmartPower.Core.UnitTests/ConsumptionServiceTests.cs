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
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>()
        {
            new()
            {
                Hour = DateTime.Parse("2022-02-21T09:00:00")
            }
        });
        _predictConsumptionService.Predict(Arg.Any<PredictionData>()).Returns(1);

        var cut = CreateTestObject();

        var res = cut.Predict(DateTime.Parse("2022-02-21T09:00:00"), DateTime.Parse("2022-02-21T10:00:00"));

        res.First().WattPerHour.Should().Be(1);
    }

    [Fact]
    public void PredictNoAnswerFromPredictionServiceShouldLastWeekConsumption()
    {
        var testDate = DateTime.Parse("2022-02-21T09:00:00");
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>()
        {
            new()
            {
                Hour = testDate
            }
        });
        _predictConsumptionService.Predict(Arg.Any<PredictionData>()).Returns<int?>(_ => null);
        _realTimeReadingRepository.GetConsumption(Arg.Is<DateTime>(testDate.AddDays(-7))).Returns(1);

        var cut = CreateTestObject();

        var res = cut.Predict(testDate, testDate.AddMinutes(59));

        res.First().WattPerHour.Should().Be(1);
    }

    [Fact]
    public void PredictNoAnswerFromPredictionServiceAndNothingFromRepositoryShouldLastWeekConsumptionFromPowerMeter()
    {
        var testDate = DateTime.Parse("2022-02-21T09:00:00");
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>()
        {
            new()
            {
                Hour = testDate
            }
        });
        _predictConsumptionService.Predict(Arg.Any<PredictionData>()).Returns<int?>(_ => null);
        _realTimeReadingRepository.GetConsumption(Arg.Is<DateTime>(testDate.AddDays(-7))).Returns<int?>(_ => null);
        _powerMeterService.GetWattPerHour(Arg.Is<DateTime>(testDate.AddDays(-7))).Returns(1);

        var cut = CreateTestObject();

        var res = cut.Predict(testDate, testDate.AddMinutes(59));

        res.First().WattPerHour.Should().Be(1);
    }

    [Fact]
    public void TwoHoursInForecastShouldReturnListOfTwo()
    {
        _weatherService.GetForecast(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<WeatherItem>()
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

    private IConsumptionService CreateTestObject()
    {
        return new ConsumptionService(_powerMeterService, _weatherService, _predictConsumptionService, _realTimeReadingRepository);
    }
}