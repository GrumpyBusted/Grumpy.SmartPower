using Grumpy.SmartPower.Core.Infrastructure;

namespace Grumpy.SmartPower.Core.Consumption
{
    public class ComsumptionService : IComsumptionService
    {
        private readonly IPowerMeterService _powerMeterService;
        private readonly IWeatherService _weatherService;

        public ComsumptionService(IPowerMeterService powerMeterService, IWeatherService weatherService)
        {
            _powerMeterService = powerMeterService;
            _weatherService = weatherService;
        }

        public IEnumerable<ConsumptionItem> Predict(DateTime from, DateTime to)
        {
            _powerMeterService.GetReading(from);
            _weatherService.GetForecast(from, to);

            throw new NotImplementedException();
        }
    }
}