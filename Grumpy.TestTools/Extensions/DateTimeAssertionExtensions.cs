using FluentAssertions;
using FluentAssertions.Primitives;
using System.Text.RegularExpressions;

namespace Grumpy.TestTools.Extensions
{
    public static class DateTimeAssertionExtensions
    {
        public static AndConstraint<DateTimeAssertions> Be(this DateTimeAssertions assertion, string expected, string because = "", params object[] becauseArgs)
        {
            if (!Regex.Match(expected, "^(\\d{4})-(\\d{2})-(\\d{2})T(\\d{2}):(\\d{2}):(\\d{2})$").Success)
                throw new ArgumentException("Invalid timestamp format, use yyyy-MM-ddTHH:mm:ss", nameof(expected));

            return assertion.BeOnOrAfter(DateTime.Parse(expected), because, becauseArgs).And.BeBefore(DateTime.Parse(expected).AddSeconds(1), because, becauseArgs);
        }
    }
}