using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SmartPower.Core.Model;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower.Core.Production;

public class ProductionService : IProductionService
{
    private readonly ProductionServiceOptions _options;
    private readonly ISolarService _solarService;
    private readonly IWeatherService _weatherService;
    private readonly IPredictProductionService _predictProductionService;

    public ProductionService(IOptions<ProductionServiceOptions> options, ISolarService solarService, IWeatherService weatherService, IPredictProductionService predictProductionService)
    {
        _options = options.Value;
        _solarService = solarService;
        _weatherService = weatherService;
        _predictProductionService = predictProductionService;

        if (_options.Direction is < 90 or > 270)
            throw new OptionsValidationException(nameof(_options.Direction), typeof(ProductionServiceOptions), new List<string> { "Invalid panel direction" });
    }

    public IEnumerable<ProductionItem> Predict(DateTime from, DateTime to)
    {
        var forecast = _weatherService.GetForecast(from, to);

        foreach (var item in forecast)
        {
            var data = GetData(item);

            var wattPerHour = _predictProductionService.Predict(data);

            yield return new ProductionItem
            {
                Hour = item.Hour,
                WattPerHour = wattPerHour ?? data.Calculated
            };
        }
    }

    public ProductionData GetData(WeatherItem item)
    {
        var sunlight = _solarService.Sunlight(item.Hour);
        var altitude = _solarService.Altitude(item.Hour);
        var direction = _solarService.Direction(item.Hour);

        var res = new ProductionData
        {
            Hour = item.Hour,
            Sun = new ProductionDataSun
            {
                Sunlight = sunlight,
                Altitude = altitude,
                Direction = direction
            },
            Weather = item
        };

        var angle = altitude < 0 ? 0 : Math.Abs(altitude + _options.Angle);
        res.Sun.VerticalAngle = angle > 90 ? 90 - angle % 90 : angle;
        res.Sun.HorizontalAngle = 90 - Math.Min(90, Math.Abs(direction - _options.Direction));
        res.Calculated = CalcWattPerHour(sunlight, res.Sun.VerticalAngle, res.Sun.HorizontalAngle, item.CloudCover);

        return res;
    }

    private int CalcWattPerHour(TimeSpan sunHour, double verticalAngle, double horizontalAngle, int cloudCover)
    {
        var production = sunHour.TotalHours * _options.Capacity;
        var angle = Math.Min(horizontalAngle, verticalAngle);
        var weatherFactor = (double)(100 - cloudCover) / 100;
        var angleFactor = angle >= 90 ? 1 : angle <= 0 ? 0 : Math.Pow(1d / (1d + Math.Exp(-8d * (angle / 90d))), 25.7d);

        return Convert.ToInt32(production * weatherFactor * angleFactor);
    }
}