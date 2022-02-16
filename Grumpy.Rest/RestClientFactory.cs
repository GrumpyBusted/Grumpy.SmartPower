using Grumpy.Rest.Interface;
using Microsoft.Extensions.Logging;

namespace Grumpy.Rest
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public RestClientFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IRestClient Instance(string baseUrl)
        {
            return new RestClient(baseUrl, _loggerFactory.CreateLogger<RestClient>());
        }
    }
}
