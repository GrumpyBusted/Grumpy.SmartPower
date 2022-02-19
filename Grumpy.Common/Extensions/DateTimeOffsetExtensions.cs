namespace Grumpy.Common.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static double TimeZoneOffset(this DateTimeOffset value)
        {
            return value.Offset.TotalHours;
        }

        public static double JulianDay(this DateTimeOffset value)
        {
            return value.Date.ToExcelDateValue() + 2415018.5 - value.TimeZoneOffset() / 24;
        }

        public static double JulianCentury(this DateTimeOffset value)
        {
            return (value.JulianDay() - 2451545) / 36525;
        }

        public static double TimePastLocalMidnight(this DateTimeOffset value)
        {
            return new DateTime(1899, 12, 30, 0, 0, 0).Add(value.TimeOfDay).ToOADate();
        }
    }
}