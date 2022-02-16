using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SolarInformation.Interface;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower.Infrastructure
{
    public class SolarService : ISolarService
    {
        private readonly SolarServiceOptions _options;
        private readonly ISolarInformation _solarInforamtion;

        public SolarService(IOptions<SolarServiceOptions> options, ISolarInformation solarInforamtion)
        {
            _options = options.Value;
            _solarInforamtion = solarInforamtion;
        }

        public double Altitude(DateTime hour)
        {
            return _solarInforamtion.AltitudePerHour(_options.Latitude, _options.Longitude, hour);
        }

        public double Direction(DateTime hour)
        {
            return _solarInforamtion.DirectionPerHour(_options.Latitude, _options.Longitude, hour);
        }

        public TimeSpan Sunlight(DateTime hour)
        {
            return _solarInforamtion.SunlightPerHour(_options.Latitude, _options.Longitude, hour);
        }
    }
}