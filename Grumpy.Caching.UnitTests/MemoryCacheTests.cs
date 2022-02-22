using System;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Grumpy.Caching.UnitTests;

public class MemoryCacheTests
{
    [Fact]
    public void MyTestMethod()
    {
        var options = new CacheOptions();
        var factory = new CacheFactory(Options.Create(options));
        var cut = factory.MemoryCacheInstance("MemoryCacheTests");
        var i = 0;

        var res = cut.TryGetIfNotSet("1", TimeSpan.FromMinutes(10), () => $"Value: {i++}");

        res.Should().Be("Value: 0");

        res = cut.TryGetIfNotSet("1", TimeSpan.FromMinutes(10), () => $"Value: {i++}");

        res.Should().Be("Value: 0");
    }            
}