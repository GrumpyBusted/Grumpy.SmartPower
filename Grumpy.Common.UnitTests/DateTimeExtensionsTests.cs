using FluentAssertions;
using Grumpy.Common.Extensions;
using System;
using Xunit;

namespace Grumpy.Common.UnitTests
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void DateOnlyFromTimestampShouldGetDate()
        {
            var value = DateTime.Parse("2022-01-02T03:04:05");

            var res = value.ToDateOnly();

            res.Should().Be(new DateOnly(2022, 1, 2));
        }

        [Fact]
        public void ToExcelDateValueShouldReturnCorrectValue()
        {
            var value = DateTime.Parse("2022-01-02T03:04:05");

            var res = value.ToExcelDateValue();

            res.Should().Be(44563.12783564815);
        }

        [Fact]
        public void ToUnixTimestampShouldReturnValue()
        {
            var value = DateTime.Parse("2022-01-02T03:04:05");

            var res = value.ToUnixTimestamp();

            res.Should().Be(1641092645);
        }
    }
}