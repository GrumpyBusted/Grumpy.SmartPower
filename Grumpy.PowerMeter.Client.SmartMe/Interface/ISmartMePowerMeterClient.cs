namespace Grumpy.PowerMeter.Client.SmartMe.Interface;

public interface ISmartMePowerMeterClient
{
    public double GetValue(DateTime dateTime);
}