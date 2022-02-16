using Grumpy.Rest.Interface;
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
            _restClient.Execute<Api.Weather.Root>(Arg.Any<RestRequest>()).Returns(new Api.Weather.Root());

            cut.GetSunInformation();
        }

        [Fact]
        public void CanGetForecast()
        {
            var cut = CreateTestObject();
            _restClient.Execute<Api.OneCall.Root>(Arg.Any<RestRequest>()).Returns(new Api.OneCall.Root());

            cut.GetForecast();
        }

        private IOpenWeatherMapClient CreateTestObject()
        {
            return new OpenWeatherMapClient(Options.Create(new OpenWeatherMapClientOptions()), _restClientFactory);
        }
    }
}