using FluentAssertions;
using Grumpy.Caching.Extensions;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Options;
using Xunit;

namespace Grumpy.Caching.UnitTests;

public class CacheFactoryTests
{
    [Fact]
    public void CanCreateFileCache()
    {
        var options = new CacheOptions
        {
            Root = ""
        }
        var cut = new CacheFactory(Options.Create(options));

        var res = cut.FileCacheInstance("Test");

        res.Should().BeOfType<FileCache>();
    }

    [Fact]
    public void CanCreateMemoryCache()
    {
        var options = new CacheOptions
        {
            Root = ""
        }
        var cut = new CacheFactory(Options.Create(options));

        var res = cut.MemoryCacheInstance("Test");

        res.Should().BeOfType<MemoryCache>();
    }
}