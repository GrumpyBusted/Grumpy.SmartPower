using System.Runtime.Caching;

namespace Grumpy.Caching;

public class MemoryCache : Cache
{
    private readonly string _name;

    internal MemoryCache(CacheOptions options, string name) : base(options)
    {
        _name = name;
    }

    protected override ObjectCache CreateCache()
    {
        return new System.Runtime.Caching.MemoryCache(_name);
    }
}