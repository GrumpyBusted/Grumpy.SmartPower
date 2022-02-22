using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Consumption;

public class ConsumptionService : IConsumptionService
{
    private readonly IPowerMeterService _powerMeterService;
    private readonly IWeatherService _weatherService;
    private readonly IPredictConsumptionService _predictConsumptionService;
    private readonly IRealTimeReadingRepository _realTimeReadingRepository;

    public ConsumptionService(IPowerMeterService powerMeterService, IWeatherService weatherService, IPredictConsumptionService predictConsumptionService, IRealTimeReadingRepository realTimeReadingRepository)
    {
        _powerMeterService = powerMeterService;
        _weatherService = weatherService;
        _predictConsumptionService = predictConsumptionService;
        _realTimeReadingRepository = realTimeReadingRepository;
    }

    public IEnumerable<ConsumptionItem> Predict(DateTime from, DateTime to)
    {
        var forecast = _weatherService.GetForecast(from, to);

        foreach (var item in forecast)
        {
            ConsumptionData data = GetData(item);

            var wattPerHour = _predictConsumptionService.Predict(data) ?? data.Consumption.LastWeek;

            yield return new ConsumptionItem
            {
                Hour = item.Hour,
                WattPerHour = wattPerHour
            };
        }
    }

    public ConsumptionData GetData(WeatherItem item)
    {
        var history = _weatherService.GetHistory(item.Hour.AddDays(-8), item.Hour.AddDays(-1)).ToList();
        var yesterday = item.Hour.AddDays(-1);
        var lastWeek = item.Hour.AddDays(-7);
        var lastWeekFromYesterday = item.Hour.AddDays(-8);

        var res = new ConsumptionData
        {
            Hour = item.Hour,
            Weather = new ConsumptionDataWeather
            {
                Forecast = item,
                Yesterday = history.FirstOrDefault(i => i.Hour == yesterday) ?? item,
                LastWeek = history.FirstOrDefault(i => i.Hour == lastWeek) ?? item,
                LastWeekFromYesterday = history.FirstOrDefault(i => i.Hour == lastWeekFromYesterday) ?? item
            },
            Consumption = new ConsumptionDataConsumption
            {
                Yesterday = _realTimeReadingRepository.GetConsumption(yesterday) ?? _powerMeterService.GetWattPerHour(yesterday),
                LastWeek = _realTimeReadingRepository.GetConsumption(lastWeek) ?? _powerMeterService.GetWattPerHour(lastWeek),
                LastWeekFromYesterday = _realTimeReadingRepository.GetConsumption(lastWeekFromYesterday) ?? _powerMeterService.GetWattPerHour(lastWeekFromYesterday),
            }
        };

        res.Consumption.WeekFactor = res.Consumption.LastWeekFromYesterday == 0 || res.Consumption.LastWeek == 0 ? 0 : (double)res.Consumption.Yesterday / res.Consumption.LastWeekFromYesterday;

        return res;
    }
}