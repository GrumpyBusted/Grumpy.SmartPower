using System.Globalization;

namespace Grumpy.Common.Extensions;

public static class GenericExtensions
{
    public static string CsvHeader<T>(this T value, char separator = ',')
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var properties = value.GetType().GetProperties();

        var list = properties.Select(prop => CsvField(prop.Name, separator)).ToList();

        return string.Join(separator, list);
    }

    public static string CsvRecord<T>(this T value, char separator = ',')
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var properties = value.GetType().GetProperties();

        var list = new List<string>();

        foreach (var prop in properties)
        {
            string? item;

            if (prop.PropertyType == typeof(DateTime))
                item = ((DateTime?)prop.GetValue(value))?.ToString("s");
            else if (prop.PropertyType == typeof(double))
                item = ((double?)prop.GetValue(value))?.ToString(CultureInfo.InvariantCulture);
            else if (prop.PropertyType == typeof(float))
                item = ((float?)prop.GetValue(value))?.ToString(CultureInfo.InvariantCulture);
            else
                item = prop.GetValue(value)?.ToString();

            list.Add(CsvField(item ?? "", separator));
        }

        return string.Join(separator, list);
    }

    private static string CsvField(string value, char separator)
    {
        value = value.Replace("\"", "\\\"");

        if (value.Contains('"') || value.Contains(' ') || value.Contains(separator))
            value = "\"" + value + "\"";

        return value;
    }
}