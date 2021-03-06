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
            var data = GetData(item);

            var wattPerHour = _predictConsumptionService.Predict(data);

            wattPerHour ??= data.Consumption.Yesterday;
            wattPerHour ??= data.Consumption.LastWeek;
            wattPerHour ??= _realTimeReadingRepository.GetConsumption(from.AddHours(-1));
            wattPerHour ??= _powerMeterService.GetWattPerHour(from.AddDays(-7));

            yield return new ConsumptionItem
            {
                Hour = item.Hour,
                WattPerHour = wattPerHour.Value 
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
                Yesterday = history.FirstOrDefault(i => i.Hour == yesterday),
                LastWeek = history.FirstOrDefault(i => i.Hour == lastWeek),
                LastWeekFromYesterday = history.FirstOrDefault(i => i.Hour == lastWeekFromYesterday)
            },
            Consumption = new ConsumptionDataConsumption
            {
                Yesterday = _realTimeReadingRepository.GetConsumption(yesterday),
                LastWeek = _realTimeReadingRepository.GetConsumption(lastWeek),
                LastWeekFromYesterday = _realTimeReadingRepository.GetConsumption(lastWeekFromYesterday)
            }
        };

        if ((res.Consumption.LastWeekFromYesterday ?? 0) == 0 || (res.Consumption.Yesterday ?? 0) == 0)
            res.Consumption.WeekFactor = 1;
        else
            res.Consumption.WeekFactor = (double)(res.Consumption.Yesterday ?? 0) / (res.Consumption.LastWeekFromYesterday ?? 0);

        return res;
    }
}