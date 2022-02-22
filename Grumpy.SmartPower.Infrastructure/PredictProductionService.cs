﻿using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Production;
using Grumpy.SmartPower.Infrastructure.PredictProductionModel;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Grumpy.SmartPower.Infrastructure;

public class PredictProductionService : IPredictProductionService
{
    private readonly PredictProductionServiceOptions _options;
    private readonly MLContext _context;
    private readonly Lazy<PredictionEngine<Input, Output>?> _predictionEngine;

    public PredictProductionService(IOptions<PredictProductionServiceOptions> options)
    {
        _options = options.Value;
        _context = new MLContext();
        _predictionEngine = new Lazy<PredictionEngine<Input, Output>?>(GetPredictionEngine);
    }

    private PredictionEngine<Input, Output>? GetPredictionEngine()
    {
        if (!File.Exists(_options.ModelPath))
            return null;

        var model = _context.Model.Load(_options.ModelPath, out _);
        
        return _context.Model.CreatePredictionEngine<Input, Output>(model);
    }

    public int? Predict(ProductionData data)
    {
        var input = MapToModelInput(data);

        var output = _predictionEngine.Value?.Predict(input);

        if (float.IsNaN(output?.Score ?? float.NaN))
            return null;

        return output?.Score == null ? null : (int)Math.Round(output.Score, 0);
    }

    public void FitModel(ProductionData data, int actualWattPerHour)
    {
        var input = MapToModelInput(data, actualWattPerHour);

        if (!File.Exists(_options.DataPath))
        {
            var f = Path.GetDirectoryName(_options.DataPath) ?? ".";

            if (f != "" && !Directory.Exists(f))
                Directory.CreateDirectory(f);

            File.WriteAllText(_options.DataPath, input.CsvHeader(';') + Environment.NewLine);
        }

        File.AppendAllText(_options.DataPath, input.CsvRecord(';') + Environment.NewLine);

        var dataView = _context.Data.LoadFromTextFile<Input>(_options.DataPath, ';', true);

        var pipeline = _context.Transforms.Conversion.ConvertType(nameof(Input.WattPerHour))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Hour)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Month)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Sunlight)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.SunAltitude)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.SunDirection)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Temperature)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.CloudCover)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WindSpeed)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Calculated)))
            .Append(_context.Transforms.Concatenate("Features",
                nameof(Input.Hour),
                nameof(Input.Month),
                nameof(Input.Sunlight),
                nameof(Input.SunAltitude),
                nameof(Input.SunDirection),
                nameof(Input.Temperature),
                nameof(Input.CloudCover),
                nameof(Input.WindSpeed),
                nameof(Input.Calculated)))
            .Append(_context.Transforms.CopyColumns("Label", nameof(Input.WattPerHour)))
            .Append(_context.Regression.Trainers.FastForest());
        var model = pipeline.Fit(dataView);

        var folder = Path.GetDirectoryName(_options.ModelPath) ?? ".";

        if (folder != "" && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        _context.Model.Save(model, dataView.Schema, _options.ModelPath);
    }

    private static Input MapToModelInput(ProductionData data, int wattPerHour = 0)
    {
        return new Input
        {
            Hour = data.Hour.Hour,
            Month = data.Hour.Month,
            Sunlight = data.Sun.Sunlight,
            SunAltitude = data.Sun.Altitude,
            SunDirection = data.Sun.Direction,
            Temperature = data.Weather.Temperature,
            CloudCover = data.Weather.CloudCover,
            WindSpeed = data.Weather.WindSpeed,
            Calculated = data.Calculated,
            WattPerHour = wattPerHour
        };
    }
}