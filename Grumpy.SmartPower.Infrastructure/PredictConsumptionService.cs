using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Infrastructure.PredictConsumptionModel;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Grumpy.SmartPower.Infrastructure;

public class PredictConsumptionService : IPredictConsumptionService
{
    private readonly PredictConsumptionServiceOptions _options;
    private readonly MLContext _context;
    private PredictionEngine<Input, Output>? _predictionEngine;
    private bool _disposed;

    public PredictConsumptionService(IOptions<PredictConsumptionServiceOptions> options)
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

    public int? Predict(ConsumptionData data)
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

    public void FitModel(ConsumptionData data, int actualWattPerHour)
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
            .Append(_context.Transforms.Categorical.OneHotEncoding(nameof(Input.Weekday)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Hour)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.Month)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WattPerHourYesterday)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WattPerHourLastWeek)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WattPerHourLastWeekFromYesterday)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WattPerHourWeekFactor)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.TemperatureForecast)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WindSpeedForecast)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.TemperatureYesterday)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WindSpeedYesterday)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.TemperatureLastWeek)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WindSpeedLastWeek)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.TemperatureLastWeekFromYesterday)))
            .Append(_context.Transforms.Conversion.ConvertType(nameof(Input.WindSpeedLastWeekFromYesterday)))
            .Append(_context.Transforms.Concatenate("Features",
                nameof(Input.Weekday),
                nameof(Input.Hour),
                nameof(Input.Month),
                nameof(Input.WattPerHourYesterday),
                nameof(Input.WattPerHourLastWeek),
                nameof(Input.WattPerHourLastWeekFromYesterday),
                nameof(Input.WattPerHourWeekFactor),
                nameof(Input.TemperatureForecast),
                nameof(Input.WindSpeedForecast),
                nameof(Input.TemperatureYesterday),
                nameof(Input.WindSpeedYesterday),
                nameof(Input.TemperatureLastWeek),
                nameof(Input.WindSpeedLastWeek),
                nameof(Input.TemperatureLastWeekFromYesterday),
                nameof(Input.WindSpeedLastWeekFromYesterday)))
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

    private static Input? MapToModelInput(ConsumptionData data, int wattPerHour = 0)
    {
        if (data.Weather.Forecast == null ||
            data.Weather.Yesterday == null ||
            data.Weather.LastWeek == null ||
            data.Weather.LastWeekFromYesterday == null ||
            data.Consumption.Yesterday == null ||
            data.Consumption.LastWeek == null ||
            data.Consumption.LastWeekFromYesterday == null)
            return null;

        return new Input
        {
            Weekday = data.Hour.DayOfWeek.ToString(),
            Hour = data.Hour.Hour,
            Month = data.Hour.Month,
            WattPerHourYesterday = data.Consumption.Yesterday.Value,
            WattPerHourLastWeek = data.Consumption.LastWeek.Value,
            WattPerHourLastWeekFromYesterday = data.Consumption.LastWeekFromYesterday.Value,
            WattPerHourWeekFactor = data.Consumption.WeekFactor,
            TemperatureForecast = data.Weather.Forecast.Temperature,
            WindSpeedForecast = data.Weather.Forecast.WindSpeed,
            TemperatureYesterday = data.Weather.Yesterday.Temperature,
            WindSpeedYesterday = data.Weather.Yesterday.WindSpeed,
            TemperatureLastWeek = data.Weather.LastWeek.Temperature,
            WindSpeedLastWeek = data.Weather.LastWeek.WindSpeed,
            TemperatureLastWeekFromYesterday = data.Weather.LastWeekFromYesterday.Temperature,
            WindSpeedLastWeekFromYesterday = data.Weather.LastWeekFromYesterday.WindSpeed,
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