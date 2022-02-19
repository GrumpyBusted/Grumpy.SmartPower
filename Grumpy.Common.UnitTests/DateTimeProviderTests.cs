using System;
using FluentAssertions;
using Grumpy.Common.Interface;
using Xunit;

namespace Grumpy.Common.UnitTests
{
    public class DateTimeProviderTests
    {
        [Fact]
        public void NowShouldReturnNow()
        {
            var cut = CreateTestObject();

            var res = cut.Now;

            res.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void TodayShouldReturnToday()
        {
            var cut = CreateTestObject();

            var res = cut.Today;

            res.Should().Be(DateTime.Today);
        }

        private static IDateTimeProvider CreateTestObject()
        {
            return new DateTimeProvider();
        }

    }
}