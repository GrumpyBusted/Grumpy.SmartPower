namespace Grumpy.SmartPower.Core.Infrastructure
{
    public interface ISolarService
    {
        double Altitude(DateTime hour);
        double Direction(DateTime hour);
        TimeSpan Sunlight(DateTime hour);
    }
}