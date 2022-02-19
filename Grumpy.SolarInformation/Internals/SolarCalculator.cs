// This class is inspired by SolarTimes in Solar Calculator 3.1.0 by Daniel M. Porrey, see https://github.com/porrey/Solar-Calculator

using Grumpy.Common.Extensions;

namespace Grumpy.SolarInformation.Internals
{
    internal class SolarCalculator
    {
        private readonly DateTimeOffset _timestamp;
        private readonly Angle _latitude;
        private readonly Angle _longitude;

        public SolarCalculator(DateTimeOffset timestamp, Angle latitude, Angle longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "The value for Latitude must be between -90° and 90°.");

            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "The value for longitude must be between -180° and 180°.");

            _timestamp = timestamp;
            _latitude = latitude;
            _longitude = longitude;
        }

        public DateTime Sunrise => _timestamp.Date.Add(TimeSpan.FromDays(SolarNoon.TimeOfDay.TotalDays - HourAngleSunrise * 4 / 1440));

        public DateTime Sunset => _timestamp.Date.Add(TimeSpan.FromDays(SolarNoon.TimeOfDay.TotalDays + HourAngleSunrise * 4 / 1440));

        public Angle SolarElevation => new Angle(90) - SolarZenith;

        public Angle SolarAzimuth
        {
            get
            {
                var angle = Angle.FromRadians(Math.Acos((Math.Sin(_latitude.Radians) * Math.Cos(SolarZenith.Radians) - Math.Sin(SolarDeclination.Radians)) / (Math.Cos(_latitude.Radians) * Math.Sin(SolarZenith.Radians))));

                return HourAngleDegrees > 0.0 ? Angle.Reduce(angle + new Angle(180.0)) : Angle.Reduce(new Angle(540.0) - angle);
            }
        }

        private static Angle AtmosphericRefraction => new(0.833);

        private Angle SunGeometricMeanLongitude => new(280.46646 + _timestamp.JulianCentury() * (36000.76983 + _timestamp.JulianCentury() * 0.0003032) % 360);

        private Angle SunMeanAnomaly => new(357.52911 + _timestamp.JulianCentury() * (35999.05029 - 0.0001537 * _timestamp.JulianCentury()));

        private double EccentricityOfEarthOrbit => 0.016708634 - _timestamp.JulianCentury() * (0.000042037 + 0.0000001267 * _timestamp.JulianCentury());

        private Angle SunEquationOfCenter => new(Math.Sin(SunMeanAnomaly.Radians) * (1.914602 - _timestamp.JulianCentury() * (0.004817 + 0.000014 * _timestamp.JulianCentury())) + Math.Sin((SunMeanAnomaly * 2).Radians) * (0.019993 - 0.000101 * _timestamp.JulianCentury()) + Math.Sin((SunMeanAnomaly * 3).Radians) * 0.000289);

        private Angle SunTrueLongitude => SunGeometricMeanLongitude + SunEquationOfCenter;

        private Angle SunApparentLongitude => new(SunTrueLongitude - 0.00569 - 0.00478 * Math.Sin(new Angle(125.04 - 1934.136 * _timestamp.JulianCentury()).Radians));

        private Angle MeanEclipticObliquity => 23 + (26 + (21.448 - _timestamp.JulianCentury() * (46.815 + _timestamp.JulianCentury() * (0.00059 - _timestamp.JulianCentury() * 0.001813))) / 60) / 60;

        private Angle ObliquityCorrection => new(MeanEclipticObliquity + 0.00256 * Math.Cos(((Angle)(125.04 - 1934.136 * _timestamp.JulianCentury())).Radians));

        private Angle SolarDeclination => Angle.FromRadians(Math.Asin(Math.Sin(ObliquityCorrection.Radians) * Math.Sin(SunApparentLongitude.Radians)));

        private double VarY => Math.Tan((ObliquityCorrection / 2).Radians) * Math.Tan((ObliquityCorrection / 2).Radians);

        private double EquationOfTime => 4 * (VarY * Math.Sin(2 * SunGeometricMeanLongitude.Radians) - 2 * EccentricityOfEarthOrbit * Math.Sin(SunMeanAnomaly.Radians) + 4 * EccentricityOfEarthOrbit * VarY * Math.Sin(SunMeanAnomaly.Radians) * Math.Cos(2 * SunGeometricMeanLongitude.Radians) - 0.5 * VarY * VarY * Math.Sin(4 * SunGeometricMeanLongitude.Radians) - 1.25 * EccentricityOfEarthOrbit * EccentricityOfEarthOrbit * Math.Sin((SunMeanAnomaly * 2).Radians)).ToDegrees();

        private Angle HourAngleSunrise => Angle.FromRadians(Math.Acos(Math.Cos((90 + AtmosphericRefraction).Radians) / (Math.Cos(_latitude.Radians) * Math.Cos(SolarDeclination.Radians)) - Math.Tan(_latitude.Radians) * Math.Tan(SolarDeclination.Radians)));

        private DateTime SolarNoon => _timestamp.Date.Add(TimeSpan.FromDays((720 - 4 * _longitude - EquationOfTime + _timestamp.TimeZoneOffset() * 60) / 1440));

        private Angle SolarZenith => Angle.FromRadians(Math.Acos(Math.Sin(_latitude.Radians) * Math.Sin(SolarDeclination.Radians) + Math.Cos(_latitude.Radians) * Math.Cos(SolarDeclination.Radians) * Math.Cos(HourAngleDegrees.Radians)));

        private double TrueSolarTime => _timestamp.TimePastLocalMidnight() * 1440 + EquationOfTime + 4 * _longitude - 60 * _timestamp.TimeZoneOffset() % 1440;

        private Angle HourAngleDegrees
        {
            get
            {
                var temp = TrueSolarTime / 4;

                return temp < 0 ? temp + 180 : temp - 180;
            }
        }
    }
}