using Grumpy.HouseBattery.Client.Sonnen.Dto;

namespace Grumpy.HouseBattery.Client.Sonnen.Helpers
{
    public static class OperatingModeExtensions
    {
        public static string ToApiString(this OperatingMode operatingMode)
        {
            return operatingMode switch
            {
                OperatingMode.Manual => "1",
                OperatingMode.SelfConsumption => "2",
                OperatingMode.TimeOfUse => "10",
                _ => throw new ArgumentException("Unknown Operating Mode", nameof(operatingMode))
            };
        }
    }
}