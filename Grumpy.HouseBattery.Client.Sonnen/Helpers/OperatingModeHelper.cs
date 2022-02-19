using Grumpy.HouseBattery.Client.Sonnen.Dto;

namespace Grumpy.HouseBattery.Client.Sonnen.Helpers
{
    public static class OperatingModeHelper
    {
        public static OperatingMode Parse(string operatingMode)
        {
            return operatingMode switch
            {
                "1" => OperatingMode.Manual,
                "2" => OperatingMode.SelfConsumption,
                "10" => OperatingMode.TimeOfUse,
                _ => throw new ArgumentException("Unknown Operating Mode", nameof(operatingMode))
            };
        }
    }
}