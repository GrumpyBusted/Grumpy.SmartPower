using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq;
using Grumpy.SmartPower.Core.Dto;
using Xunit;

namespace Grumpy.PowerPrice.Client.EnergyDataService.IntegrationTests
{
    public class EnergyDataServiceClientTests
    {
        [Fact]
        public void GetPricesShouldReturnList()
        {
            var cut = CreateTestObject();

            var from = DateTime.Now.Date;
            var to = from.AddDays(1) - TimeSpan.FromSeconds(1);

            var res = cut.GetPrices(PriceArea.DK2, from, to).ToList();

            res.Should().HaveCount(24);
            res.First().Price.Should().BeGreaterThan(0);
        }

        private static IEnergyDataServiceClient CreateTestObject()
        {
            return new EnergyDataServiceClient(new RestClientFactory(Substitute.For<ILoggerFactory>()));
        }
    }
}