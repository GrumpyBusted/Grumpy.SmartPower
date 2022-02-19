using System.Text.Json;

namespace Grumpy.Json
{
    public static class GenericExtensions
    {
        public static string SerializeToJson<T>(this T value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}