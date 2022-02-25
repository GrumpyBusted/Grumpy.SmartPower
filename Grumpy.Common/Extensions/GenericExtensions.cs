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
            string item;

            var obj = prop.GetValue(value);

            if (obj == null)
                item = "";
            else if (prop.PropertyType == typeof(DateTime))
                item = ((DateTime)obj).ToUniversalTime().ToString("O");
            else if (prop.PropertyType == typeof(double))
                item = ((double)obj).ToString(CultureInfo.InvariantCulture);
            else if (prop.PropertyType == typeof(float))
                item = ((float)obj).ToString(CultureInfo.InvariantCulture);
            else
                item = obj.ToString() ?? "";

            list.Add(CsvField(item, separator));
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