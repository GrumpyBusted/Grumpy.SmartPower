using FluentAssertions;
using Grumpy.TestTools.Extensions;
using System;
using Xunit;

namespace Grumpy.TestTools.UnitTests
{
    public class DateTimeAssertionExtensionsTests
    {
        [Fact]
        public void SameTimestampShouldMatch()
        {
            var actual = DateTime.Parse("2022-02-11T21:01:02");

            actual.Should().Be("2022-02-11T21:01:02");
        }

        [Fact]
        public void TimestampInSameSecondShouldMatch()
        {
            var actual = DateTime.Parse("2022-02-11T21:01:02.999");

            actual.Should().Be("2022-02-11T21:01:02");
        }

        [Fact]
        public void InvalidTimestampInExpectedShouldThrow()
        {
            var actual = DateTime.Parse("2022-02-11T21:01:02.999");

            var act = () => actual.Should().Be("2022-02-11T21:01:02.1");

            act.Should().Throw<ArgumentException>();
        }
    }
}