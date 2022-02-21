﻿using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class PredictConsumptionServiceTests
{
    [Fact]
    public void TrainModelShouldCreateFiles()
    {
        var options = new PredictConsumptionServiceOptions()
        {
            DataPath = $"ConsumptionData-{Guid.NewGuid()}.csv",
            ModelPath = $"ConsumptionModel-{Guid.NewGuid()}.zip"
        };

        var cut = new PredictConsumptionService(Options.Create(options));

        var data = new PredictionData()
        {
            Hour = DateTime.Parse("2022-02-21T09:00:00"),
            Consumption = new()
            {
                Yesterday = 10,
                LastWeek = 20,
                LastWeekFromYesterday = 30
            },
            Weather = new()
            {
                Forecast = new()
                {
                    Temperature = 1,
                    CloudCover = 2,
                    WindSpeed = 3,
                },
                Yesterday = new()
                {
                    Temperature = 4,
                    CloudCover = 5,
                    WindSpeed = 6,
                },
                LastWeek = new()
                {
                    Temperature = 7,
                    CloudCover = 8,
                    WindSpeed = 9,
                },
                LastWeekFromYesterday = new()
                {
                    Temperature = 10,
                    CloudCover = 11,
                    WindSpeed = 12,
                }
            }
        };

        cut.TrainModel(data, 100);

        File.Exists(options.DataPath).Should().BeTrue();
        File.Exists(options.ModelPath).Should().BeTrue();
    }

    [Fact]
    public void AfterTrainingShouldPredict()
    {
        var options = new PredictConsumptionServiceOptions()
        {
            DataPath = $"ConsumptionData-{Guid.NewGuid()}.csv",
            ModelPath = $"ConsumptionModel-{Guid.NewGuid()}.zip"
        };

        var cut = new PredictConsumptionService(Options.Create(options));

        var data = new PredictionData()
        {
            Hour = DateTime.Parse("2022-02-21T09:00:00"),
            Consumption = new()
            {
                Yesterday = 10,
                LastWeek = 20,
                LastWeekFromYesterday = 30
            },
            Weather = new()
            {
                Forecast = new()
                {
                    Temperature = 1,
                    CloudCover = 2,
                    WindSpeed = 3,
                },
                Yesterday = new()
                {
                    Temperature = 4,
                    CloudCover = 5,
                    WindSpeed = 6,
                },
                LastWeek = new()
                {
                    Temperature = 7,
                    CloudCover = 8,
                    WindSpeed = 9,
                },
                LastWeekFromYesterday = new()
                {
                    Temperature = 10,
                    CloudCover = 11,
                    WindSpeed = 12,
                }
            }
        };

        for(var i = 1; i <= 40; i++)
        {
            data.Consumption.Yesterday = i;
            cut.TrainModel(data, 100 + i);
        }

        data.Consumption.Yesterday = 1;
        var res = cut.Predict(data);

        res.Should().BeInRange(100, 120);
    }
}
