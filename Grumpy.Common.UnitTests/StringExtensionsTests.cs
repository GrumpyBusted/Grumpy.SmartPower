using System;
using FluentAssertions;
using Grumpy.Common.Extensions;
using Xunit;

namespace Grumpy.Common.UnitTests;

public class StringExtensionsTests
{
    [Fact]
    public void CsvHeaderShouldList()
    {
        const string value = "\"Anders Busted-Janum\";1973;1973-10-25T09:14:00;123.45";

        var res = value.ParseCsv<TestClass>(';');

        res.Name.Should().Be("Anders Busted-Janum");
        res.Year.Should().Be(1973);
        res.Birthday.Should().Be(DateTime.Parse("1973-10-25T09:14:00"));
        res.Salary.Should().Be(123.45);
    }

    [Fact]
    public void IsCsvHeaderShouldBeTrue()
    {
        const string value = "Name;Year;Birthday;Salary";

        var res = value.IsCsvHeader<TestClass>(';');

        res.Should().BeTrue();
    }

    [Fact]
    public void IsCsvHeaderShouldBeFalse()
    {
        const string value = "\"Anders Busted-Janum\";1973;1973-10-25T09:14:00;123.45";

        var res = value.IsCsvHeader<TestClass>(';');

        res.Should().BeFalse();
    }

    [Fact]
    public void ValidFileNameShouldWork()
    {
        const string value = "Test:File\\Name";

        var res = value.ValidFileName();

        res.Should().Be("Test_File_Name");
    }

    private class TestClass
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string Name { get; set; } = "";
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int Year { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DateTime Birthday { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public double Salary { get; set; }
    }
}