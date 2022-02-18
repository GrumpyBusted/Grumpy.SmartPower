using FluentAssertions;
using Grumpy.Caching.Extensions;
using System.Runtime.Caching;
using System.Threading;
using Xunit;

namespace Grumpy.Caching.UnitTests;

public class ObjectCacheExtensionsTests
{
    [Fact]
    public void TryGetIfNotSetWithLowTimeSpanShouldNotUseCache()
    {
        var cache = new MemoryCache("Test");

        var first = cache.TryGetIfNotSet("Key", System.TimeSpan.FromMilliseconds(1), () => 1);

        first.Should().Be(1);

        Thread.Sleep(100);

        first = cache.TryGetIfNotSet("Key", System.TimeSpan.FromMilliseconds(1), () => 2);

        first.Should().Be(2);
    }

    [Fact]
    public void TryGetIfNotSetWitHeightTimeSpanShouldUseCache()
    {
        var cache = new MemoryCache("Test");

        var first = cache.TryGetIfNotSet("Key", System.TimeSpan.FromHours(1), () => 1);

        first.Should().Be(1);

        first = cache.TryGetIfNotSet("Key", System.TimeSpan.FromHours(1), () => 2);

        first.Should().Be(1);
    }
}