using FluentAssertions;
using Grumpy.Json.UnitTests.Helper;
using System;
using Xunit;

namespace Grumpy.Json.UnitTests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void CanDeserializeStringShouldReturnObject()
        {
            const string json = "{\"Name\":\"Anders\",\"Birthday\":\"1973-10-25T10:00:00\",\"Year\":1973}";

            var obj = json.DeserializeFromJson<TestClass>();

            obj.Name.Should().Be("Anders");
            obj.Birthday.Should().Be(new DateTime(1973, 10, 25, 10, 0, 0));
            obj.Year.Should().Be(1973);
        }
    }
}