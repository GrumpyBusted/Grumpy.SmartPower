using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen.Dto;
using Grumpy.HouseBattery.Client.Sonnen.Helpers;
using Xunit;

namespace Grumpy.HouseBattery.Client.Sonnen.UnitTests
{
    public class OperatingModeHelperTests
    {
        [Fact]
        public void ParseToOperatingModeShouldMapCorrectly()
        {
            OperatingModeHelper.Parse("1").Should().Be(OperatingMode.Manual);
            OperatingModeHelper.Parse("2").Should().Be(OperatingMode.SelfConsumption);
            OperatingModeHelper.Parse("10").Should().Be(OperatingMode.TimeOfUse);
        }
    }
}