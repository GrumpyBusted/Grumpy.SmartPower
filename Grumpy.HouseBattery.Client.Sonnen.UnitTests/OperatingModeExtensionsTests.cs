using FluentAssertions;
using Grumpy.HouseBattery.Client.Sonnen.Dto;
using Grumpy.HouseBattery.Client.Sonnen.Helpers;
using Xunit;

namespace Grumpy.HouseBattery.Client.Sonnen.UnitTests;

public class OperatingModeExtensionsTests
{
    [Fact]
    public void OperatingModeToStringShouldMapCorrectly()
    {
        OperatingMode.Manual.ToApiString().Should().Be("1");
        OperatingMode.SelfConsumption.ToApiString().Should().Be("2");
        OperatingMode.TimeOfUse.ToApiString().Should().Be("10");
    }
}