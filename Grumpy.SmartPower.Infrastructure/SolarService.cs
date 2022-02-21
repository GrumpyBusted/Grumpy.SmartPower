using Grumpy.SmartPower.Core.Infrastructure;
using Grumpy.SolarInformation.Interface;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower.Infrastructure;

public class SolarService : ISolarService
{
    private readonly SolarServiceOptions _options;
    private readonly ISolarInformation _solarInformation;

    public SolarService(IOptions<SolarServiceOptions> options, ISolarInformation solarInformation)
    {
        _options = options.Value;
        _solarInformation = solarInformation;
    }

    public double Altitude(DateTime hour)
    {
        return _solarInformation.AltitudePerHour(_options.Latitude, _options.Longitude, hour);
    }

    public double Direction(DateTime hour)
    {
        return _solarInformation.DirectionPerHour(_options.Latitude, _options.Longitude, hour);
    }

    public TimeSpan Sunlight(DateTime hour)
    {
        return _solarInformation.SunlightPerHour(_options.Latitude, _options.Longitude, hour);
    }
}