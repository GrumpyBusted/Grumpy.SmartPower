using FluentAssertions;
using Grumpy.Common.Extensions;
using Grumpy.TestTools.Extensions;
using Xunit;

namespace Grumpy.Common.UnitTests;

public class IntExtensionsTests
{
    [Fact]
    public void MaxWhenFirstLargerShouldReturnFirst()
    {
        const int value = 1;

        var res = value.UnixTimestampToDateTime();

        res.Should().Be("1970-01-01T01:00:01");
    }
}