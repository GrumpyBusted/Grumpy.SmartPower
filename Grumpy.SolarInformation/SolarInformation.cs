using Grumpy.Common.Extensions;
using Grumpy.Common.Helpers;
using Grumpy.SolarInformation.Interface;
using Grumpy.SolarInformation.Internals;

namespace Grumpy.SolarInformation
{
    public class SolarInformation : ISolarInformation
    {
        public DateTime Sunrise(double latitude, double longitude, DateOnly date)
        {
            return Sunrise(latitude, longitude, date, date.TimeZoneOffset());
        }

        public DateTime Sunrise(double latitude, double longitude, DateOnly date, TimeSpan timeZoneOffset)
        {
            DateTimeOffset dateTimeOffset = new(date.Year, date.Month, date.Day, 0, 0, 0, timeZoneOffset);
            SolarCalculator solarTimes = new(dateTimeOffset, latitude, longitude);

            return solarTimes.Sunrise;
        }

        public DateTime Sunset(double latitude, double longitude, DateOnly date)
        {
            return Sunset(latitude, longitude, date, date.TimeZoneOffset());
        }

        public DateTime Sunset(double latitude, double longitude, DateOnly date, TimeSpan timeZoneOffset)
        {
            DateTimeOffset dateTimeOffset = new(date.Year, date.Month, date.Day, 0, 0, 0, timeZoneOffset);
            SolarCalculator solarTimes = new(dateTimeOffset, latitude, longitude);

            return solarTimes.Sunset;
        }

        public TimeSpan SunlightPerHour(double latitude, double longitude, DateTime dateTime)
        {
            DateTime from = new(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
            DateTime to = from.AddHours(1);

            return Sunlight(latitude, longitude, from, to);
        }

        public TimeSpan Sunlight(double latitude, double longitude, DateTime from, DateTime to)
        {
            var sunrise = Sunrise(latitude, longitude, from.ToDateOnly());
            var sunset = Sunset(latitude, longitude, from.ToDateOnly());

            return TimeSpanMath.Max(DateTimeMath.Min(to, sunset) - DateTimeMath.Max(from, sunrise), TimeSpan.Zero);
        }

        public double Altitude(double latitude, double longitude, DateTime dateTime)
        {
            SolarCalculator solarTimes = new(dateTime, latitude, longitude);

            return solarTimes.SolarElevation;
        }

        public double Altitude(double latitude, double longitude, DateTime from, DateTime to)
        {
            return (Altitude(latitude, longitude, from) + Altitude(latitude, longitude, to) + Altitude(latitude, longitude, from.Add((to - from) / 2))) / 3;
        }

        public double AltitudePerHour(double latitude, double longitude, DateTime dateTime)
        {
            DateTime from = new(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
            DateTime to = from.AddHours(1);

            return Altitude(latitude, longitude, from, to);
        }

        public double Direction(double latitude, double longitude, DateTime dateTime)
        {
            SolarCalculator solarTimes = new(dateTime, latitude, longitude);

            return solarTimes.SolarAzimuth;
        }

        public double Direction(double latitude, double longitude, DateTime from, DateTime to)
        {
            return (Direction(latitude, longitude, from) + Direction(latitude, longitude, to) + Direction(latitude, longitude, from.Add((to - from) / 2))) / 3;
        }

        public double DirectionPerHour(double latitude, double longitude, DateTime dateTime)
        {
            DateTime from = new(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
            DateTime to = from.AddHours(1);

            return Direction(latitude, longitude, from, to);
        }
    }
}