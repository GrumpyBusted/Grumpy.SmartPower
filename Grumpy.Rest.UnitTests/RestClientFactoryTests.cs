using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Grumpy.Rest.UnitTests
{
    public class RestClientFactoryTests
    {
        [Fact]
        public void CanUseRestClientFactory()
        {
            var cut = new RestClientFactory(Substitute.For<ILoggerFactory>());

            var res = cut.Instance("http://0.0.0.0");

            res.Should().BeOfType<RestClient>();
        }
    }
}