using Grumpy.Caching.Interface;

namespace Grumpy.Caching.TestMocks;

public class TestCacheFactory : ICacheFactory
{
    public static ICacheFactory Instance => new TestCacheFactory();

    private TestCacheFactory() { }

    public ICache FileCacheInstance(string name)
    {
        return new TestCache();
    }

    public ICache MemoryCacheInstance(string name)
    {
        return new TestCache();
    }
}