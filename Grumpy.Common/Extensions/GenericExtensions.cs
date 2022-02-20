using System.Globalization;

namespace Grumpy.Common.Extensions
{
    public static class GenericExtensions
    {
        public static string CsvHeader<T>(this T value, char seperator = ',')
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var properties = value.GetType().GetProperties();

            var list = new List<string>();

            foreach (var prop in properties)
            {
                list.Add(CsvField(prop.Name ?? "", seperator));
            }

            return String.Join(seperator, list);
        }

        public static string CsvRecord<T>(this T value, char seperator = ',')
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var properties = value.GetType().GetProperties();

            var list = new List<string>();

            foreach (var prop in properties)
            {
                string? item = null;

                if (prop.PropertyType == typeof(DateTime))
                    item = ((DateTime?)prop.GetValue(value))?.ToString("s");
                else if (prop.PropertyType == typeof(double))
                    item = ((double?)prop.GetValue(value))?.ToString(CultureInfo.InvariantCulture);
                else if (prop.PropertyType == typeof(float))
                    item = ((float?)prop.GetValue(value))?.ToString(CultureInfo.InvariantCulture);
                else
                    item = prop.GetValue(value)?.ToString();

                list.Add(CsvField(item ?? "", seperator));
            }

            return String.Join(seperator, list);
        }

        private static string CsvField(string value, char seperator)
        {
            value = (value ?? "").Replace("\"", "\\\"");

            if (value.Contains('"') || value.Contains(' ') || value.Contains(seperator))
                value = "\"" + value + "\"";

            return value;
        }
    }
}