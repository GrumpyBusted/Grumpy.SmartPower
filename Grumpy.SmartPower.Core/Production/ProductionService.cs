using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower.Core.Production
{
    public class ProductionService : IProductionService
    {
        private readonly ProductionServiceOptions _options;
        private readonly ISolarService _solarService;
        private readonly IWeatherService _weatherService;

        public ProductionService(IOptions<ProductionServiceOptions> options, ISolarService solarService, IWeatherService weatherService)
        {
            _options = options.Value;
            _solarService = solarService;
            _weatherService = weatherService;

            if (_options.Direction < 90 || _options.Direction > 270)
                throw new OptionsValidationException(nameof(_options.Direction), typeof(ProductionServiceOptions), new List<string>() { "Invalid panel direction" });
        }

        public IEnumerable<ProductionItem> Forecast(DateTime from, DateTime to)
        {
            var forecast = _weatherService.GetForecast(from, to);

            foreach (var hour in forecast)
            {
                var sun = _solarService.Sunlight(hour.Hour);
                var altitude = _solarService.Altitude(hour.Hour);
                var direction = _solarService.Direction(hour.Hour);

                var item = new ProductionItem
                {
                    Hour = hour.Hour,
                    WattHour = CalcWattHour(sun, altitude, direction, hour.CloudCover)
                };

                yield return item;
            }
        }

        private int CalcWattHour(TimeSpan sunHour, double altitude, double direction, int cloudCover)
        {
            var production = sunHour.TotalHours * _options.Capacity;
            var verticalAngle = altitude < 0 ? 0 : Math.Abs(altitude + _options.Angle);
            verticalAngle = verticalAngle > 90 ? 90 - verticalAngle % 90 : verticalAngle;
            var horizontalAngle = 90 - Math.Min(90, Math.Abs(direction - _options.Direction));
            var angle = Math.Min(horizontalAngle, verticalAngle);
            var weatherFactor = (double)(100 - cloudCover) / 100;
            var angleFactor = angle >= 90 ? 1 : angle <= 0 ? 0 : Math.Pow(1d / (1d + Math.Exp(-8d * (angle / 90d))), 25.7d);

            return Convert.ToInt32(production * weatherFactor * angleFactor);
        }
    }
}