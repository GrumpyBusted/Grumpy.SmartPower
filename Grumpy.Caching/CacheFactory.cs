using Grumpy.Caching.Interface;
using Microsoft.Extensions.Options;

namespace Grumpy.Caching;

public class CacheFactory : ICacheFactory
{
    private readonly CacheOptions _options;

    public CacheFactory(IOptions<CacheOptions> options)
    {
        _options = options.Value;
    }

    public ICache FileCacheInstance(string name)
    {
        return new FileCache(_options, name);
    }

    public ICache MemoryCacheInstance(string name)
    {
        return new MemoryCache(_options, name);
    }
}