using System.Globalization;
using System.Text;

namespace Grumpy.Common.Extensions;

public static class StringExtensions
{
    public static T ParseCsv<T>(this string value, char separator = ',') where T : new()
    {
        var res = new T();

        var list = value.Split(separator);

        var properties = res.GetType().GetProperties();

        var index = 0;

        foreach (var prop in properties)
        {
            var field = list[index++];

            if (field[0] == '\"' && field[^1] == '\"')
                field = field[1..^1];

            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            prop.SetValue(res, Convert.ChangeType(field, type, CultureInfo.InvariantCulture));
        }

        return res;
    }

    public static bool IsCsvHeader<T>(this string value, char separator = ',') where T : new()
    {
        var res = new T();

        var header = res.CsvHeader(separator);

        return header == value;
    }

    private static readonly Lazy<char[]> Invalids = new(Path.GetInvalidFileNameChars());

    public static string ValidFileName(this string value)
    {
        var sb = new StringBuilder(value.Length);
        var changed = false;

        foreach (var c in value)
        {
            if (Invalids.Value.Contains(c))
            {
                changed = true;
                sb.Append('_');
            }
            else
                sb.Append(c);
        }

        if (sb.Length == 0)
            return "_";

        return changed ? sb.ToString() : value;
    }
}