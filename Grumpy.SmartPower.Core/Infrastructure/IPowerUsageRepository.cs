namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IPowerUsageRepository
{
    public void Save(DateTime hour, int consumption, int production, int gridFeedIn, int gridFeedOut, int meterReading, int batteryLevel, int batteryCurrent, double price);
}