namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlow
    {
        PowerItem? Get(DateTime hour);
        PowerItem? First();
        IEnumerable<PowerItem> All();
        int ChargeBattery(DateTime hour, int value);
        int DischargeBattery(DateTime hour, int value);
        int MovePower(DateTime from, DateTime to, int value);
        void ChargeExtraPower();
        void DistributeInitialBatteryPower();
        void ChargeFromGrid();
        void DistributeBatteryPower();
        double Price();
    }
}