namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IRealTimeReadingRepository
{
    public void Save(DateTime dateTime, int consumption, int production);
    public int? GetConsumption(DateTime hour);
    public int? GetProduction(DateTime hour);
}