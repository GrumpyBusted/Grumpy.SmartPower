namespace Grumpy.Common.Extensions;

public static class DoubleExtensions
{
    public static double ToDegrees(this double value)
    {
        return value * (180 / Math.PI);
    }

    public static double ToRadians(this double value)
    {
        return value * Math.PI / 180;
    }
}