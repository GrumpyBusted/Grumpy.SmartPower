namespace Grumpy.SmartPower.Core.Consumption
{
    public class PredictionData
    {
        public DateTime Hour { get; set; }
        public PredictionWeatherData Weather { get; set; } = new();
        public PredictionConsumptionData Consumption { get; set; } = new();
    }
}