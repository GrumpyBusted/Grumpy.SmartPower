namespace Grumpy.SolarInformation.Interface;

public interface ISolarInformation
{
    double Altitude(double latitude, double longitude, DateTime dateTime);
    double Altitude(double latitude, double longitude, DateTime from, DateTime to);
    double AltitudePerHour(double latitude, double longitude, DateTime dateTime);
    double Direction(double latitude, double longitude, DateTime dateTime);
    double Direction(double latitude, double longitude, DateTime from, DateTime to);
    double DirectionPerHour(double latitude, double longitude, DateTime dateTime);
    TimeSpan Sunlight(double latitude, double longitude, DateTime from, DateTime to);
    TimeSpan SunlightPerHour(double latitude, double longitude, DateTime dateTime);
    DateTime Sunrise(double latitude, double longitude, DateOnly date);
    DateTime Sunrise(double latitude, double longitude, DateOnly date, TimeSpan timeZoneOffset);
    DateTime Sunset(double latitude, double longitude, DateOnly date);
    DateTime Sunset(double latitude, double longitude, DateOnly date, TimeSpan timeZoneOffset);
}