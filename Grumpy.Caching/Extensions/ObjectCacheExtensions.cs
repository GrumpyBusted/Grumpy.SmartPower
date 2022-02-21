using System.Runtime.Caching;

namespace Grumpy.Caching.Extensions;

public static class ObjectCacheExtensions
{
    public static T TryGetIfNotSet<T>(this ObjectCache cache, string key, TimeSpan timeout, Func<T> func)
    {
        T res;
        var value = cache.Get(key);

        if (value == null)
        {
            res = func();

            if (res != null)
                cache.Set(key, res, DateTimeOffset.Now.Add(timeout));
        }
        else
            res = (T)value;

        return res;
    }
}