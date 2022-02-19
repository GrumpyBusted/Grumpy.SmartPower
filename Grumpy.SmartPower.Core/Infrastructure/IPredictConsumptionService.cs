using Grumpy.SmartPower.Core.Consumption;

namespace Grumpy.SmartPower.Core.Infrastructure
{
    public interface IPredictConsumptionService
    {
        int Predict(PredictionData data);
        void TrainModel(PredictionData data, int actualWattPerHour);
    }
}