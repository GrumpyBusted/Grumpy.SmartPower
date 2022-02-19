using Grumpy.Rest.Interface;
using Grumpy.Weather.Client.OpenWeatherMap.Api.OneCall;
using Grumpy.Weather.Client.OpenWeatherMap.Api.Weather;
using Grumpy.Weather.Client.OpenWeatherMap.Interface;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestSharp;
using Xunit;

namespace Grumpy.Weather.Client.OpenWeatherMap.UnitTests
{
    public class OpenWeatherMapClientTests
    {
        private readonly IRestClientFactory _restClientFactory = Substitute.For<IRestClientFactory>();
        private readonly IRestClient _restClient = Substitute.For<IRestClient>();

        public OpenWeatherMapClientTests()
        {
            _restClientFactory.Instance(Arg.Any<string>()).Returns(_restClient);
        }

        [Fact]
        public void CanGetSunInformation()
        {
            var cut = CreateTestObject();
            _restClient.Execute<WeatherRoot>(Arg.Any<RestRequest>()).Returns(new WeatherRoot());

            cut.GetSunInformation();
        }

        [Fact]
        public void CanGetForecast()
        {
            var cut = CreateTestObject();
            _restClient.Execute<OneCallRoot>(Arg.Any<RestRequest>()).Returns(new OneCallRoot());

            cut.GetForecast();
        }

        private IOpenWeatherMapClient CreateTestObject()
        {
            return new OpenWeatherMapClient(Options.Create(new OpenWeatherMapClientOptions()), _restClientFactory);
        }
    }
}