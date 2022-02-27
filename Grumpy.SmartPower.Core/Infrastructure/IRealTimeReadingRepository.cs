namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IRealTimeReadingRepository
{
    public void Save(DateTime dateTime, int consumption, int production, int GridFeedIn);
    public int? GetConsumption(DateTime hour);
    public int? GetProduction(DateTime hour);
    public int? GetGridFeedIn(DateTime hour);
    public int? GetGridFeedOut(DateTime hour);
}