namespace Grumpy.SmartPower.Core.Internal;

internal class Item
{
    public DateTime Hour { get; set; }
    public int Production { get; set; }
    public int Consumption { get; set; }
    public double Price { get; set; }
    public int BatteryLevel { get; set; }
    public int MissingPower { get; set; }
    public int ExtraPower { get; set; }
    public int GridCharge { get; set; }
    public int Charge { get; set; }
    public int Discharge { get; internal set; }
    public int GridFeedOut { get; internal set; }
    public int SaveForLater { get; internal set; }
}