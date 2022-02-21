using System;
using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class TestClass
    {
        public string Name { get; set; } = "";
        public int Year { get; set; }
        public DateTime Birthday { get; set; }
        public double Salary { get; set; }
    }
}