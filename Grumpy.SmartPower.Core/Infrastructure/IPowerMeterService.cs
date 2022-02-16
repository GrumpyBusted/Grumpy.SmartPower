namespace Grumpy.SmartPower.Core.Infrastructure
{
    public interface IPowerMeterService
    {
        double GetReading(DateTime dateTime);
    }
}