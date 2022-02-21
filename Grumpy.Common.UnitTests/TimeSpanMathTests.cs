using FluentAssertions;
using Grumpy.Common.Helpers;
using System;
using Xunit;

namespace Grumpy.Common.UnitTests;

public class TimeSpanMathTests
{
    [Fact]
    public void MaxWhenFirstLargerShouldReturnFirst()
    {
        var value1 = TimeSpan.FromHours(2);
        var value2 = TimeSpan.FromHours(1);

        var res = TimeSpanMath.Max(value1, value2);

        res.Should().Be(value1);
    }

    [Fact]
    public void MaxWhenSecondLargerShouldReturnSecond()
    {
        var value1 = TimeSpan.FromHours(1);
        var value2 = TimeSpan.FromHours(2);

        var res = TimeSpanMath.Max(value1, value2);

        res.Should().Be(value2);
    }
}