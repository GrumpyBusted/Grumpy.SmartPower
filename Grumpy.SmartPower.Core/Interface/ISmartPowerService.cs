namespace Grumpy.SmartPower.Core.Interface
{
    public interface ISmartPowerService
    {
        void Execute(DateTime now);
        void SaveData(DateTime now);
    }
}