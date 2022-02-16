using Grumpy.Common.Extensions;
using Grumpy.Rest.Interface;
using Grumpy.SmartPower.Core.Model;
using Grumpy.Weather.Client.VisualCrossing.Api.Timeline;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Globalization;

namespace Grumpy.Weather.Client.VisualCrossing
{
    public class VisualCrossingWeatherClient : IVisualCrossingWeatherClient
    {
        private readonly VisualCrossingWeatherClientOptions _options;
        private readonly IRestClientFactory _restClientFactory;

        public VisualCrossingWeatherClient(IOptions<VisualCrossingWeatherClientOptions> options, IRestClientFactory restClientFactory)
        {
            _options = options.Value;
            _restClientFactory = restClientFactory;
        }

        public IEnumerable<WeatherItem> Get(DateOnly from, DateOnly to)
        {
            var client = _restClientFactory.Instance("https://weather.visualcrossing.com/VisualCrossingWebServices/rest");

            var request = new RestRequest($"services/timeline/{_options.Latitude.ToString("F", CultureInfo.InvariantCulture)},{_options.Longitude.ToString("F", CultureInfo.InvariantCulture)}/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}", Method.Get)
                .AddQueryParameter("unitGroup", "metric")
                .AddQueryParameter("include", "hours")
                .AddQueryParameter("key", _options.ApiKey)
                .AddQueryParameter("contentType", "json")
                .AddQueryParameter("elements", "datetimeEpoch,temp,windspeed,cloudcover");

            var response = client.Execute<Root>(request);

            return MapToList(response);
        }

        private static IEnumerable<WeatherItem> MapToList(Root response)
        {
            foreach(var day in response.Days)
            {
                foreach (var hour in day.Hours)
                {
                    yield return new WeatherItem()
                    {
                        Hour = hour.DateTime.UnixTimestampToDateTime(),
                        Temperature = hour.Temperature,
                        CloudCover = (int)Math.Round(hour.CloudCover, 0),
                        WindSpeed = hour.WindSpeed
                    };
                }
            }
        }
    }
}