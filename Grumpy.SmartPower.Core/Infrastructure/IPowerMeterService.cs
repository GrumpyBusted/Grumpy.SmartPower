namespace Grumpy.SmartPower.Core.Infrastructure
{
    public interface IPowerMeterService
    {
        int GetUsagePerHour(DateTime hour);
        double GetReading(DateTime dateTime);
    }
}