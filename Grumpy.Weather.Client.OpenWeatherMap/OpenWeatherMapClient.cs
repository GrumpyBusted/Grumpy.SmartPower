using Grumpy.Common.Extensions;
using Grumpy.Rest.Interface;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Weather.Client.OpenWeatherMap.Api.OneCall;
using Grumpy.Weather.Client.OpenWeatherMap.Api.Weather;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Grumpy.Weather.Client.OpenWeatherMap.UnitTests")]

namespace Grumpy.Weather.Client.OpenWeatherMap
{
    public class OpenWeatherMapClient : IOpenWeatherMapClient
    {
        private readonly OpenWeatherMapClientOptions _options;
        private readonly IRestClientFactory _restClientFactory;

        public OpenWeatherMapClient(IOptions<OpenWeatherMapClientOptions> options, IRestClientFactory restClientFactory)
        {
            _options = options.Value;
            _restClientFactory = restClientFactory;
        }

        public SunInformation GetSunInformation()
        {
            using var client = CreateClient();

            var request = CreateRequest("weather", Method.Get);

            var response = client.Execute<WeatherRoot>(request);

            var res = new SunInformation
            {
                Sunrise = response.SystemInformation.Sunrise.UnixTimestampToDateTime(),
                Sunset = response.SystemInformation.Sunset.UnixTimestampToDateTime()
            };

            return res;
        }

        public IEnumerable<WeatherItem> GetForecast()
        {
            using var client = CreateClient();

            // ReSharper disable once StringLiteralTypo
            var request = CreateRequest("onecall", Method.Get)
                .AddQueryParameter("exclude", "current,minutely,daily,alerts");

            var response = client.Execute<OneCallRoot>(request);

            return response.Forecast.Select(point => new WeatherItem
            {
                Hour = point.DateTime.UnixTimestampToDateTime(),
                Temperature = point.Temperature,
                CloudCover = point.Clouds,
                WindSpeed = point.WindSpeed
            }).ToList();
        }

        private IRestClient CreateClient()
        {
            return _restClientFactory.Instance("https://api.openweathermap.org/data/2.5");
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            var restRequest = new RestRequest(resource, method)
                .AddQueryParameter("lat", _options.Latitude)
                .AddQueryParameter("lon", _options.Longitude)
                .AddQueryParameter("units", "metric")
                .AddQueryParameter("appid", _options.ApiKey);

            return restRequest;
        }
    }
}