using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService;
using Grumpy.Rest;
using Grumpy.SmartPower.Core.Dtos;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.IntegrationTests
{
    public class PowerPriceServiceTests
    {
        [Fact]
        public void GetPricesShouldReturnList()
        {
            var cut = CreateTestObject();

            var res = cut.GetPrices(PriceArea.DK2, DateTime.Now, DateTime.Now.AddDays(1));

            res.Should().HaveCount(24);
        }

        private static IPowerPriceService CreateTestObject()
        {
            var client = new EnergyDataServiceClient(new RestClientFactory(Substitute.For<ILoggerFactory>()));

            return new PowerPriceService(client);
        }
    }
}