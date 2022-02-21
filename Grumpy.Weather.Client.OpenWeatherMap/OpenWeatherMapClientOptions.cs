namespace Grumpy.Weather.Client.OpenWeatherMap;

public class OpenWeatherMapClientOptions
{
    public double Latitude { get; set; } = 0;
    public double Longitude { get; set; } = 0;
    public string ApiKey { get; set; } = "";
}