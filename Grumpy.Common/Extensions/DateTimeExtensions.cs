namespace Grumpy.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateOnly ToDateOnly(this DateTime dateTime)
        {
            return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        public static double ToExcelDateValue(this DateTime value)
        {
            return value.Date <= new DateTime(1900, 1, 1) ? 1 + (value.ToOADate() - Math.Floor(value.ToOADate())) : value.ToOADate();
        }
    }
}

