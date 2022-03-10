namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlow
    {
        PowerHour? Get(DateTime hour);
        PowerHour? First();
        PowerHour? Last();
        IEnumerable<PowerHour> All();
        void Add(DateTime hour, int consumption, int production, double price);
        int Charge(DateTime hour, int value);
        int Charge(PowerHour item, int value);
        int Discharge(DateTime hour, int value);
        int Discharge(PowerHour item, int value);
        int Move(DateTime from, DateTime to, int value);
        int Move(PowerHour source, PowerHour target, int value);
    }
}