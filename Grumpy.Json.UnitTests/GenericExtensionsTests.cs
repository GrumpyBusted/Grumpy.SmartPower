using FluentAssertions;
using Grumpy.Json.UnitTests.Helper;
using System;
using Xunit;

namespace Grumpy.Json.UnitTests;

public class GenericExtensionsTests
{
    [Fact]
    public void CanSerializeObjectShouldReturnString()
    {
        var obj = new TestClass
        {
            Name = "Anders",
            Birthday = new DateTime(1973, 10, 25, 10, 0, 0),
            Year = 1973
        };

        var res = obj.SerializeToJson();

        res.Should().Be("{\"Name\":\"Anders\",\"Birthday\":\"1973-10-25T10:00:00\",\"Year\":1973}");
    }
}