namespace Grumpy.Common.Extensions;

public static class DateOnlyExtensions
{
    public static TimeSpan TimeZoneOffset(this DateOnly value)
    {
        return ((DateTimeOffset)value.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Local)).Offset;
    }
}