using Grumpy.SmartPower.Core.Consumption;

namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IPredictConsumptionService : IDisposable
{
    int? Predict(ConsumptionData data);
    void FitModel(ConsumptionData data, int actualWattPerHour);
}