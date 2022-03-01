namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlow
    {
        public PowerItem? Get(DateTime hour);
        public PowerItem? First();
        public IEnumerable<PowerItem> All();
        public int ChargeBattery(DateTime hour, int value);
        public int DischargeBattery(DateTime hour, int value);
        public int MovePower(DateTime from, DateTime to, int value);
        public void DistributeExtraSolarPower();
        void DistributeInitialBatteryPower();
    }
}