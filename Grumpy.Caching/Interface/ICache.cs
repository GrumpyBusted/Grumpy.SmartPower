namespace Grumpy.Caching.Interface;

public interface ICache
{
    T TryGetIfNotSet<T>(string key, TimeSpan timeout, Func<T> func);
}