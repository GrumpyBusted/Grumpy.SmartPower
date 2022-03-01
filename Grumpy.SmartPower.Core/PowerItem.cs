namespace Grumpy.SmartPower.Core
{
    public class PowerItem
    {
        public DateTime Hour { get; set; }
        public int Production { get; set; }
        public int Consumption { get; set; }
        public double Price { get; set; }
        public int BatteryLevel { get; set; }
        public int Power { get; set; }
        public int Charge { get; set; }
    }
}