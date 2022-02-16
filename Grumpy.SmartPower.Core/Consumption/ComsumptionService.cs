using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using System.Linq;

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
            var forecast = _weatherService.GetForecast(from, to);
            var history = _weatherService.GetHistory(from.AddDays(-8), to.AddDays(-7))
                .Concat(_weatherService.GetHistory(from.AddDays(-1), to.AddDays(-1)));

            foreach (var item in forecast)
            {
                var lastWeek = _powerMeterService.GetUsagePerHour(item.Hour.AddDays(-7));
                var changeInConsumptionFromLastWeek = _powerMeterService.GetUsagePerHour(item.Hour.AddDays(-1)) - _powerMeterService.GetUsagePerHour(item.Hour.AddDays(-8));

                yield return new ConsumptionItem()
                {
                    Hour = item.Hour,
                    WattPerHour = lastWeek + changeInConsumptionFromLastWeek
                };
            }
        }
    }
}