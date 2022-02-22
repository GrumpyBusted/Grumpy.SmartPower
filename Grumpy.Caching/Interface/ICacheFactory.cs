namespace Grumpy.Caching.Interface;

public interface ICacheFactory
{
    ICache FileCacheInstance(string name);
    ICache MemoryCacheInstance(string name);
}