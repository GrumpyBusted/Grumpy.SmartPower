using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Reflection;

namespace Grumpy.SmartPower.Infrastructure
{
    public class ModelInput
    {
        [ColumnName("Temperature"), LoadColumn(0)]
        public double Temperature { get; set; }
        [ColumnName("WindSpeed"), LoadColumn(1)]
        public double WindSpeed { get; set; }
        [ColumnName("CloudCover"), LoadColumn(2)]
        public int CloudCover { get; set; }
        [ColumnName("WattHours"), LoadColumn(3)]
        public int WattHours { get; set; }
    }

    public class ModelOutput
    {
        public float Score { get; set; }
    }

    public class PredictConsumptionService
    {
        private readonly PredictConsumptionServiceOptions _options;
        private readonly MLContext _context;
        private readonly Lazy<ITransformer> _model;
        private readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> _predictionEngine;

        public PredictConsumptionService(IOptions<PredictConsumptionServiceOptions> options)
        {
            _options = options.Value;
            _context = new MLContext();
            _model = new Lazy<ITransformer>(() => GetModel(_context, _options.ModelPath));
            _predictionEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => GetPredictionEngine(_context, _model.Value));
        }

        private static ITransformer GetModel(MLContext context, string path)
        {
            return context.Model.Load(path, out var _);
        }

        private static PredictionEngine<ModelInput, ModelOutput> GetPredictionEngine(MLContext context, ITransformer model)
        {
            return context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        }

        public int Predict(PredictionData data)
        {
            var input = MapToModelInput(data);

            var res = _predictionEngine.Value.Predict(input);

            return (int)Math.Round((double)res.Score, 0);

        }

        public void TrainModel(PredictionData newData, int actualWattHours)
        {
            if (!File.Exists(_options.DataPath))
                File.Create(_options.DataPath);

            var input = MapToModelInput(newData);

            File.AppendAllText(_options.DataPath, ToCsv(input, ';') + Environment.NewLine);

            var data = _context.Data.LoadFromTextFile<ModelInput>(_options.DataPath, ';');

            var pipeline = _context.Transforms.Conversion.ConvertType(nameof(ModelInput.WattHours))
                .Append(_context.Transforms.Conversion.ConvertType(nameof(ModelInput.Temperature)))
                .Append(_context.Transforms.Conversion.ConvertType(nameof(ModelInput.WindSpeed)))
                .Append(_context.Transforms.Conversion.ConvertType(nameof(ModelInput.CloudCover)))
                .Append(_context.Transforms.Concatenate("Features", nameof(ModelInput.Temperature), nameof(ModelInput.WindSpeed), nameof(ModelInput.CloudCover)))
                .Append(_context.Transforms.CopyColumns("Label", nameof(ModelInput.WattHours)))
                .Append(_context.Regression.Trainers.FastForest("Label", "Features"));

            var model = pipeline.Fit(data);

            _context.Model.Save(model, data.Schema, _options.ModelPath);
        }

        private static string ToCsv<T>(T obj, char separator)
        {
            var properties = typeof(T).GetProperties();
            var res = "";
            //foreach(var property in properties.Select(p => new { order = p.GetCustomAttribute(typeof(LoadColumnAttribute)), value = p.GetValue(p)}).Where(x => x.order != null).OrderBy(s => s.order))
            //{
            //    res += property.value.ToString() + separator;
            //}

            ////string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            ////StringBuilder csvdata = new StringBuilder();
            ////csvdata.AppendLine(header);

            ////foreach (var o in objectlist)
            ////    csvdata.AppendLine(ToCsvFields(separator, fields, o));

            return res;
        }

        private static ModelInput MapToModelInput(PredictionData data, int wattHours = 0)
        {
            return new ModelInput()
            {
                Temperature = data.Temperature,
                WindSpeed = data.WindSpeed,
                CloudCover = data.CloudCover
            };
        }
    }

    public class PredictionData
    {
        public double Temperature { get; internal set; }
        public double WindSpeed { get; internal set; }
        public int CloudCover { get; internal set; }
    }

    public class PredictConsumptionServiceOptions
    {
        public string ModelPath { get; set; } = "";
        public string DataPath { get; set; } = "";
    }
}