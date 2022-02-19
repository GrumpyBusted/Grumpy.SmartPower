using FluentAssertions;
using Grumpy.Common.Extensions;
using System;
using Xunit;

namespace Grumpy.Common.UnitTests
{
    public class DateTimeOffsetExtensionsTests
    {
        [Fact]
        public void TimeZoneOffsetShouldBeOneHour()
        {
            var value = new DateTimeOffset(2022, 1, 2, 3, 4, 5, TimeSpan.FromHours(1));

            var res = value.TimeZoneOffset();

            res.Should().Be(1);
        }

        [Fact]
        public void TimeZoneOffsetShouldBeOneHourAndAHalf()
        {
            var value = new DateTimeOffset(2022, 1, 2, 3, 4, 5, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));

            var res = value.TimeZoneOffset();

            res.Should().Be(1.5);
        }

        [Fact]
        public void JulianDayShouldBeCorrect()
        {
            var value = new DateTimeOffset(2022, 1, 2, 3, 4, 5, TimeSpan.FromHours(1));

            var res = value.JulianDay();

            res.Should().BeApproximately(2459581.458333, 0.00001);
        }

        [Fact]
        public void JulianCenturyShouldBeCorrect()
        {
            var value = new DateTimeOffset(2022, 1, 2, 3, 4, 5, TimeSpan.FromHours(1));

            var res = value.JulianCentury();

            res.Should().BeApproximately(0.22002623773671426, 0.00000000001);
        }

        [Fact]
        public void TimePastLocalMidnightShouldBeCorrect()
        {
            var value = new DateTimeOffset(2022, 1, 2, 1, 0, 0, TimeSpan.Zero);

            var res = value.TimePastLocalMidnight();

            res.Should().BeApproximately(0.0416666666, 0.00001);
        }
    }
}