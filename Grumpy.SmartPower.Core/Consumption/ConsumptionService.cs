using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using System.Linq;

namespace Grumpy.SmartPower.Core.Consumption
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly IPowerMeterService _powerMeterService;
        private readonly IWeatherService _weatherService;
        private readonly IPredictConsumptionService _predictConsumptionService;

        public ConsumptionService(IPowerMeterService powerMeterService, IWeatherService weatherService, IPredictConsumptionService predictConsumptionService)
        {
            _powerMeterService = powerMeterService;
            _weatherService = weatherService;
            _predictConsumptionService = predictConsumptionService;
        }

        public IEnumerable<ConsumptionItem> Predict(DateTime from, DateTime to)
        {
            var forecast = _weatherService.GetForecast(from, to);
            var history = _weatherService.GetHistory(from.AddDays(-8), to.AddDays(-1));

            foreach (var item in forecast)
            {
                var yesterday = item.Hour.AddDays(-1);
                var lastWeek = item.Hour.AddDays(-7);
                var lastWeekFromYesterday = item.Hour.AddDays(-8);
                
                var data = new PredictionData()
                {
                    Hour = item.Hour,
                    Weather = new PredictionWeatherData()
                    {
                        Forecast = item,
                        Yesterday = history.First(i => i.Hour == yesterday),
                        LastWeek = history.First(i => i.Hour == lastWeek),
                        LastWeekFromYesterday = history.First(i => i.Hour == lastWeekFromYesterday)
                    },
                    Consumption = new PredictionConsumptionData()
                    {
                        Yesterday = _powerMeterService.GetWattPerHour(yesterday),
                        LastWeek = _powerMeterService.GetWattPerHour(lastWeek),
                        LastWeekFromYesterday = _powerMeterService.GetWattPerHour(lastWeekFromYesterday)
                    }
                };

                yield return new ConsumptionItem()
                {
                    Hour = item.Hour,
                    WattPerHour = _predictConsumptionService.Predict(data)
                };
            }
        }
    }
}