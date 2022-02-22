using System.Runtime.Caching;
using Grumpy.Caching.Extensions;
using Grumpy.Caching.Interface;

namespace Grumpy.Caching;

public abstract class Cache : ICache
{
    protected readonly CacheOptions Options;
    private readonly Lazy<ObjectCache> _cache;

    protected Cache(CacheOptions options)
    {
        Options = options;
        _cache = new Lazy<ObjectCache>(CreateCache);
    }

    protected abstract ObjectCache CreateCache();

    public T TryGetIfNotSet<T>(string key, TimeSpan timeout, Func<T> func)
    {
        return _cache.Value.TryGetIfNotSet(key, timeout, func);
    }
}