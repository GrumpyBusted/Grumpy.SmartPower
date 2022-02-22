using Grumpy.Caching.Interface;

namespace Grumpy.Caching.TestMocks;

public class TestCache : ICache
{
    internal TestCache() { } 
    
    public T TryGetIfNotSet<T>(string key, TimeSpan timeout, Func<T> func)
    {
        return func();
    }
}