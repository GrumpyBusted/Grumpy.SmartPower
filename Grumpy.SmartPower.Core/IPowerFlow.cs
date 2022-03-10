namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlow
    {
        PowerHour? Get(DateTime hour);
        PowerHour? First();
        IEnumerable<PowerHour> All();
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