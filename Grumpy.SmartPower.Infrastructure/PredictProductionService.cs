using Grumpy.Common.Extensions;
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
    private PredictionEngine<Input, Output>? _predictionEngine;
    private bool _disposed;

    public PredictProductionService(IOptions<PredictProductionServiceOptions> options)
    {
        _options = options.Value;
        _context = new MLContext();
        _predictionEngine = null;
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
        if (!File.Exists(_options.DataPath))
            return null;

        if (File.ReadAllLines(_options.DataPath).Length < 168)
            return null;

        var input = MapToModelInput(data);

        if (input == null)
            return null;

        _predictionEngine ??= GetPredictionEngine();

        var output = _predictionEngine?.Predict(input);

        if (float.IsNaN(output?.Score ?? float.NaN))
            return null;

        return (int)Math.Round(output?.Score ?? 0, 0);
    }

    public void FitModel(ProductionData data, int actualWattPerHour)
    {
        var input = MapToModelInput(data, actualWattPerHour);

        if (input == null)
            return;

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
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.HorizontalAngle)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.VerticalAngle)))
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
                nameof(Input.HorizontalAngle),
                nameof(Input.VerticalAngle),
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

        _predictionEngine?.Dispose();
        _predictionEngine = null;
    }

    private static Input? MapToModelInput(ProductionData data, int wattPerHour = 0)
    {
        var utc = data.Hour.ToUniversalTime();

        if (data.Weather == null)
            return null;

        return new Input
        {
            Hour = utc.Hour,
            Month = utc.Month,
            Sunlight = (int)data.Sun.Sunlight.TotalSeconds,
            SunAltitude = data.Sun.Altitude,
            SunDirection = data.Sun.Direction,
            HorizontalAngle = data.Sun.HorizontalAngle,
            VerticalAngle = data.Sun.VerticalAngle,
            Temperature = data.Weather.Temperature,
            CloudCover = data.Weather.CloudCover,
            WindSpeed = data.Weather.WindSpeed,
            Calculated = data.Calculated,
            WattPerHour = wattPerHour
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) 
            return;

        if (disposing)
            _predictionEngine?.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}