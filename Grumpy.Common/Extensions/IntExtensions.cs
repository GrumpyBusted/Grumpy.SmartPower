namespace Grumpy.Common.Extensions;

public static class IntExtensions
{
    public static DateTime UnixTimestampToDateTime(this int unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        return dateTime.AddSeconds(unixTimestamp).ToLocalTime();
    }

}