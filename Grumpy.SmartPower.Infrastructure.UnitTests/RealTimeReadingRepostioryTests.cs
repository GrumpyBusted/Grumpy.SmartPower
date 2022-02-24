using FluentAssertions;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using Grumpy.Caching.TestMocks;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class RealTimeReadingRepositoryTests
{
    [Fact]
    public void SaveInNewFileShouldCreateFile()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);

        cut.Save(DateTime.Now, 1, 2);

        File.Exists(fileName).Should().BeTrue();
    }

    [Fact]
    public void SaveInFileShouldHaveHeaderLine()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);

        cut.Save(DateTime.Now, 1, 2);

        File.ReadAllLines(fileName).First().Should().Be("DateTime;Consumption;Production");
    }

    [Fact]
    public void SaveInFileShouldHaveRecord()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);

        cut.Save(DateTime.Parse("2022-02-21T09:00:00"), 1, 2);

        File.ReadAllLines(fileName).Skip(1).First().Should().Be("2022-02-21T09:00:00.0000000;1;2");
    }

    [Fact]
    public void SaveTwiceShouldHaveThreeRecords()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);

        cut.Save(DateTime.Now, 1, 1);
        cut.Save(DateTime.Now, 2, 2);

        File.ReadAllLines(fileName).Should().HaveCount(3);
    }

    [Fact]
    public void ReadFromNonExistingFileShouldReturnEmpty()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);

        var res = cut.GetConsumption(DateTime.Parse("2022-02-19T09:00:00"));

        res.Should().BeNull();
    }

    [Fact]
    public void ReadFileWithOneRowShouldReturnValue()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);
        cut.Save(DateTime.Parse("2022-02-20T09:00:00"), 1, 2);

        var res = cut.GetConsumption(DateTime.Parse("2022-02-20T09:00:00"));

        res.Should().Be(1);
    }

    [Fact]
    public void ReadFileWithMoreRecordsShouldReturnCorrectValue()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);
        cut.Save(DateTime.Parse("2022-02-21T09:00:00"), 1, 11);
        cut.Save(DateTime.Parse("2022-02-21T09:10:00"), 3, 13);
        cut.Save(DateTime.Parse("2022-02-21T10:00:00"), 5, 15);

        var res = cut.GetConsumption(DateTime.Parse("2022-02-21T09:00:00"));

        res.Should().Be(2);
    }

    [Fact]
    public void GetProductionReadFileWithMoreRecordsShouldReturnCorrectValue()
    {
        var fileName = $"Repository\\{Guid.NewGuid()}.csv";

        var cut = CreateTestObject(fileName);
        cut.Save(DateTime.Parse("2022-02-21T09:00:00"), 1, 11);
        cut.Save(DateTime.Parse("2022-02-21T09:10:00"), 3, 13);
        cut.Save(DateTime.Parse("2022-02-21T10:00:00"), 5, 15);

        var res = cut.GetProduction(DateTime.Parse("2022-02-21T09:00:00"));

        res.Should().Be(12);
    }

    private static IRealTimeReadingRepository CreateTestObject(string path)
    {
        var options = new RealTimeReadingRepositoryOptions
        {
            RepositoryPath = path
        };

        return new RealTimeReadingRepository(Options.Create(options), TestCacheFactory.Instance);
    }
}