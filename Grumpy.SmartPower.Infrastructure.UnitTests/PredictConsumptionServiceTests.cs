using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class PredictConsumptionServiceTests
{
    [Fact]
    public void TrainModelShouldCreateFiles()
    {
        var options = new PredictConsumptionServiceOptions
        {
            DataPath = $"ConsumptionData-{Guid.NewGuid()}.csv",
            ModelPath = $"ConsumptionModel-{Guid.NewGuid()}.zip"
        };

        IPredictConsumptionService cut = new PredictConsumptionService(Options.Create(options));

        var data = new ConsumptionData
        {
            Hour = DateTime.Parse("2022-02-21T09:00:00"),
            ConsumptionDataConsumption = new ConsumptionDataConsumption
            {
                Yesterday = 10,
                LastWeek = 20,
                LastWeekFromYesterday = 30
            },
            Weather = new ConsumptionDataWeather
            {
                Forecast = new WeatherItem
                {
                    Temperature = 1,
                    CloudCover = 2,
                    WindSpeed = 3
                },
                Yesterday = new WeatherItem
                {
                    Temperature = 4,
                    CloudCover = 5,
                    WindSpeed = 6
                },
                LastWeek = new WeatherItem
                {
                    Temperature = 7,
                    CloudCover = 8,
                    WindSpeed = 9
                },
                LastWeekFromYesterday = new WeatherItem
                {
                    Temperature = 10,
                    CloudCover = 11,
                    WindSpeed = 12
                }
            }
        };

        cut.FitModel(data, 100);

        File.Exists(options.DataPath).Should().BeTrue();
        File.Exists(options.ModelPath).Should().BeTrue();
    }

    [Fact]
    public void AfterTrainingShouldPredict()
    {
        var options = new PredictConsumptionServiceOptions
        {
            DataPath = $"ConsumptionData-{Guid.NewGuid()}.csv",
            ModelPath = $"ConsumptionModel-{Guid.NewGuid()}.zip"
        };

        IPredictConsumptionService cut = new PredictConsumptionService(Options.Create(options));

        var data = new ConsumptionData
        {
            Hour = DateTime.Parse("2022-02-21T09:00:00"),
            ConsumptionDataConsumption = new ConsumptionDataConsumption
            {
                Yesterday = 10,
                LastWeek = 20,
                LastWeekFromYesterday = 30
            },
            Weather = new ConsumptionDataWeather
            {
                Forecast = new WeatherItem
                {
                    Temperature = 1,
                    CloudCover = 2,
                    WindSpeed = 3
                },
                Yesterday = new WeatherItem
                {
                    Temperature = 4,
                    CloudCover = 5,
                    WindSpeed = 6
                },
                LastWeek = new WeatherItem
                {
                    Temperature = 7,
                    CloudCover = 8,
                    WindSpeed = 9
                },
                LastWeekFromYesterday = new WeatherItem
                {
                    Temperature = 10,
                    CloudCover = 11,
                    WindSpeed = 12
                }
            }
        };

        for(var i = 1; i <= 40; i++)
        {
            data.ConsumptionDataConsumption.Yesterday = i;
            cut.FitModel(data, 100 + i);
        }

        data.ConsumptionDataConsumption.Yesterday = 1;
        var res = cut.Predict(data);

        res.Should().BeInRange(100, 120);
    }
}
