using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestSharp;
using System;
using Xunit;

namespace Grumpy.Rest.UnitTests
{
    public class RestClientTests
    {
        [Fact]
        public void ExecuteShouldThrow()
        {
            var logger = Substitute.For<ILoggerFactory>();
            var factory = new RestClientFactory(logger);
            var cut = factory.Instance("http://0.0.0.0");

            var act = () => cut.Execute<string>(new RestRequest());

            act.Should().Throw<Exception>();
        }
    }
}