using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IPredictProductionService
{
    int? Predict(ProductionData data);
    void FitModel(ProductionData data, int actualWattPerHour);
}