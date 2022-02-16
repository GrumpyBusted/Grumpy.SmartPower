using FluentAssertions;
using Grumpy.Common.Extensions;
using System;
using Xunit;

namespace Grumpy.Common.UnitTests
{
    public class DateOnlyExtensionsTests
    {
        [Fact]
        public void TimeZoneOffsetDuringStandardTimeShouldBeOneHours()
        {
            var value = new DateOnly(2022, 1, 1);

            var res = value.TimeZoneOffset();

            res.Should().Be(TimeSpan.FromHours(1));
        }

        [Fact]
        public void TimeZoneOffsetDuringDayLightSavingShouldBeTwoHours()
        {
            var value = new DateOnly(2022, 6, 1);

            var res = value.TimeZoneOffset();

            res.Should().Be(TimeSpan.FromHours(2));
        }
    }
}