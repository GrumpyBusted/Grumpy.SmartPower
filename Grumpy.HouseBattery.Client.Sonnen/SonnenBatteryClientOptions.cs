namespace Grumpy.HouseBattery.Client.Sonnen;

public class SonnenBatteryClientOptions
{
    public string Ip { get; set; } = "";
    public string ApiToken { get; set; } = "";
    public int BatterySize { get; set; }
    public int InverterLimit { get; set; }
}