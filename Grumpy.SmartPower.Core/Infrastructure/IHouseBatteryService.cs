using Grumpy.SmartPower.Core.Model;

namespace Grumpy.SmartPower.Core.Infrastructure;

public interface IHouseBatteryService
{
    public bool IsBatteryFull();
    public int GetBatteryLevel();
    public int GetBatterySize();
    public int GetBatteryCurrent();
    public int GetProduction();
    public int GetConsumption();
    public void SetMode(BatteryMode batteryMode, DateTime hour);
    public BatteryMode GetBatteryMode();
    public int InverterLimit();
    public int GetGridFeedIn();
}