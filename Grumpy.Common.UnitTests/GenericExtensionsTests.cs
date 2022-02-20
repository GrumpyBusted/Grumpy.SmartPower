using FluentAssertions;
using Grumpy.Common.Extensions;
using Grumpy.Common.Helpers;
using System;
using Xunit;

namespace Grumpy.Common.UnitTests
{
    public class GenericExtensionsTests
    {
        [Fact]
        public void CsvHeaderShouldList()
        {
            var value = new
            {
                Name = "Anders",
                Year = 1973
            };

            var res = value.CsvHeader(';');

            res.Should().Be("Name;Year");
        }

        [Fact]
        public void CsvRecordShouldList()
        {
            var value = new
            {
                Name = "Anders",
                Year = 1973,
                Birthday = DateTime.Parse("1973-10-25T09:14:00"),
                Salary = 123.45
            };

            var res = value.CsvRecord(';');

            res.Should().Be("Anders;1973;1973-10-25T09:14:00;123.45");
        }

        [Fact]
        public void CsvRecordWithQuetsShouldList()
        {
            var value = new
            {
                Name = "Anders Busted-Janum",
                Text = "Semicolon;Text",
                Comment = "With\""
            };

            var res = value.CsvRecord(';');
            
            res.Should().Be("\"Anders Busted-Janum\";\"Semicolon;Text\";\"With\\\"\"");
        }
    }
}