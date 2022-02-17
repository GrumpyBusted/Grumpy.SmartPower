namespace Grumpy.SmartPower.Core.Consumption
{
    public class PredictionData
    {
        public DateTime Hour { get; set; }
        public PredictionWeatherData Weather { get; set; } = new PredictionWeatherData();
        public PredictionConsumptionData Consumption { get; set; } = new PredictionConsumptionData();
    }
}