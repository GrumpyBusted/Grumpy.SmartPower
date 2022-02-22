using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Grumpy.Caching.UnitTests;

public class FileCacheTests
{
    [Fact]
    public void GetFromCacheShouldWork()
    {
        var options = new CacheOptions();
        var factory = new CacheFactory(Options.Create(options));
        var cut = factory.FileCacheInstance("");
        var i = 0;
 
        var res = cut.TryGetIfNotSet("1", TimeSpan.FromMinutes(10), () => $"Value: {i++}");

        res.Should().Be("Value: 0");

        res = cut.TryGetIfNotSet("1", TimeSpan.FromMinutes(10), () => $"Value: {i++}");

        res.Should().Be("Value: 0");
    }

    [Fact]
    public void UsingCacheShouldCreateCacheFolder()
    {
        var options = new CacheOptions { Root = $"FileCache-{Guid.NewGuid()}" };
        var factory = new CacheFactory(Options.Create(options));
        var name = $"Cache-{Guid.NewGuid()}";
        var cut = factory.FileCacheInstance(name);

        cut.TryGetIfNotSet("1", TimeSpan.FromMinutes(10), () => "Dummy");

        Directory.Exists($"{options.Root}\\{name}").Should().BeTrue();
    }
}