using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService;
using Grumpy.Rest;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Grumpy.SmartPower.Core.Dto;
using Xunit;
using System.Linq;

namespace Grumpy.SmartPower.Infrastructure.IntegrationTests
{
    public class PowerPriceServiceTests
    {
        [Fact]
        public void GetPricesShouldReturnList()
        {
            var cut = CreateTestObject();

            var res = cut.GetPrices(PriceArea.DK2, PriceArea.DK, DateTime.Now, DateTime.Now.AddDays(1)).ToList();

            res.Should().HaveCountGreaterThan(6);
        }

        private static IPowerPriceService CreateTestObject()
        {
            var client = new EnergyDataServiceClient(new RestClientFactory(Substitute.For<ILoggerFactory>()));

            return new PowerPriceService(client);
        }
    }
}