using FluentAssertions;
using Grumpy.SmartPower.Core.Consumption;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Xunit;
using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class PredictProductionServiceTests
{
    [Fact]
    public void TrainModelShouldCreateFiles()
    {
        var options = new PredictProductionServiceOptions
        {
            DataPath = $"ProductionData-{Guid.NewGuid()}.csv",
            ModelPath = $"ProductionModel-{Guid.NewGuid()}.zip"
        };

        IPredictProductionService cut = new PredictProductionService(Options.Create(options));

        var data = new ProductionData
        {
            Hour = DateTime.Parse("2022-02-21T09:00:00"),
            Calculated = 1,
            Sun = new ProductionDataSun()
            {
               Sunlight = TimeSpan.FromHours(0.5),
               Altitude = 3,
               Direction = 4
            },
        };

        cut.FitModel(data, 100);

        File.Exists(options.DataPath).Should().BeTrue();
        File.Exists(options.ModelPath).Should().BeTrue();
    }

    [Fact]
    public void AfterTrainingShouldPredict()
    {
        var options = new PredictProductionServiceOptions
        {
            DataPath = $"ProductionData-{Guid.NewGuid()}.csv",
            ModelPath = $"ProductionModel-{Guid.NewGuid()}.zip"
        };

        IPredictProductionService cut = new PredictProductionService(Options.Create(options));

        var data = new ProductionData
        {
            Hour = DateTime.Parse("2022-02-21T09:00:00"),
            Calculated = 1,
            Sun = new ProductionDataSun()
            {
                Sunlight = TimeSpan.FromHours(0.5),
                Altitude = 3,
                Direction = 4
            },
        };

        for(var i = 1; i <= 40; i++)
        {
            data.Calculated = i;
            cut.FitModel(data, 100 + i);
        }

        data.Calculated = 1;
        var res = cut.Predict(data);

        res.Should().BeInRange(100, 120);
    }
}
