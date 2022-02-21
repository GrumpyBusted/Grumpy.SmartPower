namespace Grumpy.Common.Helpers;

public static class DateTimeMath
{
    public static DateTime Max(DateTime value1, DateTime value2)
    {
        return value1 > value2 ? value1 : value2;
    }

    public static DateTime Min(DateTime value1, DateTime value2)
    {
        return value1 < value2 ? value1 : value2;
    }
}