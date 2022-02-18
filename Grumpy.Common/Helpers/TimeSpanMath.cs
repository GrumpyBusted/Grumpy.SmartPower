namespace Grumpy.Common.Helpers;

public static class TimeSpanMath
{
    public static TimeSpan Max(TimeSpan value1, TimeSpan value2)
    {
        return value1 > value2 ? value1 : value2;
    }
}