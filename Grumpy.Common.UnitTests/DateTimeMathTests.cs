using FluentAssertions;
using Grumpy.Common.Helpers;
using System;
using Xunit;

namespace Grumpy.Common.UnitTests;

public class DateTimeMathTests
{
    [Fact]
    public void MaxIsFirstValueShouldReturnFirst()
    {
        var value1 = DateTime.Parse("2022-01-02T03:04:05");
        var value2 = DateTime.Parse("2022-01-02T03:04:04");

        var res = DateTimeMath.Max(value1, value2);
            
        res.Should().Be(value1);
    }

    [Fact]
    public void MaxIsSecondValueShouldReturnSecond()
    {
        var value1 = DateTime.Parse("2022-01-02T03:04:05");
        var value2 = DateTime.Parse("2022-01-02T03:04:06");

        var res = DateTimeMath.Max(value1, value2);

        res.Should().Be(value2);
    }

    [Fact]
    public void MinIsFirstValueShouldReturnFirst()
    {
        var value1 = DateTime.Parse("2022-01-02T03:04:05");
        var value2 = DateTime.Parse("2022-01-02T03:04:06");

        var res = DateTimeMath.Min(value1, value2);

        res.Should().Be(value1);
    }

    [Fact]
    public void MinIsSecondValueShouldReturnSecond()
    {
        var value1 = DateTime.Parse("2022-01-02T03:04:05");
        var value2 = DateTime.Parse("2022-01-02T03:04:04");

        var res = DateTimeMath.Min(value1, value2);

        res.Should().Be(value2);
    }
}