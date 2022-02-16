using FluentAssertions;
using Grumpy.Caching.Extensions;
using System.Runtime.Caching;
using System.Threading;
using Xunit;

namespace Grumpy.Caching.UnitTests
{
    public class ObjectCacheExtensionsTests
    {
        [Fact]
        public void TryGetIfNotSetWithLowTimeSpanShouldNotUseCache()
        {
            var _cache = new MemoryCache("Test");

            var first = _cache.TryGetIfNotSet("Key", System.TimeSpan.FromMilliseconds(1), () => 1);

            first.Should().Be(1);

            Thread.Sleep(100);

            first = _cache.TryGetIfNotSet("Key", System.TimeSpan.FromMilliseconds(1), () => 2);

            first.Should().Be(2);
        }

        [Fact]
        public void TryGetIfNotSetWitHightTimeSpanShouldUseCache()
        {
            var _cache = new MemoryCache("Test");

            var first = _cache.TryGetIfNotSet("Key", System.TimeSpan.FromHours(1), () => 1);

            first.Should().Be(1);

            first = _cache.TryGetIfNotSet("Key", System.TimeSpan.FromHours(1), () => 2);

            first.Should().Be(1);
        }
    }
}