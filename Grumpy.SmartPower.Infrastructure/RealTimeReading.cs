namespace Grumpy.SmartPower.Infrastructure
{
    internal class RealTimeReading
    {
        public DateTime DateTime { get; set; }
        public int? Consumption { get; set; }
        public int? Production { get; set; }
    }
}