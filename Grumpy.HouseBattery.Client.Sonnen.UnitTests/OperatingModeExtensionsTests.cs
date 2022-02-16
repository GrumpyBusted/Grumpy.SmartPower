using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen.Dtos;
using Grumpy.HouseBattery.Client.Sonnen.Helpers;
using Xunit;

namespace Grumpy.HouseBattery.Client.Sonnen.UnitTests
{
    public class OperatingModeExtensionsTests
    {
        [Fact]
        public void OperatingModeToStringShouldMapCorrectly()
        {
            OperatingModeExtensions.ToApiString(OperatingMode.Manual).Should().Be("1");
            OperatingModeExtensions.ToApiString(OperatingMode.SelfConsumption).Should().Be("2");
            OperatingModeExtensions.ToApiString(OperatingMode.TimeOfUse).Should().Be("10");
        }
    }
}