namespace Grumpy.SmartPower.Core.Infrastructure
{
    public interface IPowerMeterService
    {
        int GetWattPerHour(DateTime hour);
        double GetReading(DateTime dateTime);
    }
}