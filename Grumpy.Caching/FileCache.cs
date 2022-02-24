using System.Runtime.Caching;
using Grumpy.Common.Extensions;

namespace Grumpy.Caching;

public class FileCache : Cache
{
    private readonly string _name;

    internal FileCache(CacheOptions options, string name) : base(options)
    {
        _name = name;
    }

    protected override ObjectCache CreateCache()
    {
        var cacheRoot = Options.Root == "" ? Path.Combine(Directory.GetCurrentDirectory(), "FileCache") : Options.Root;

        return new System.Runtime.Caching.FileCache(FileCacheManagers.Hashed, cacheRoot + $"\\{_name.ValidFileName()}", new FileCacheBinder());
    }
}