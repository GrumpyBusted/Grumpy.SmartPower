using Grumpy.Json.Exceptions;
using System.Text.Json;

namespace Grumpy.Json
{
    public static class StringExtensions
    {
        public static T DeserializeFromJson<T>(this string value)
        {
            return JsonSerializer.Deserialize<T>(value) ?? throw new JsonDeserializeException(typeof(T), value);
        }
    }
}