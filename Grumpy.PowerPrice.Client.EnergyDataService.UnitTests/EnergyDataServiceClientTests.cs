using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.Rest.Interface;
using NSubstitute;
using System;
using Grumpy.SmartPower.Core.Dto;
using Xunit;

namespace Grumpy.PowerPrice.Client.EnergyDataService.UnitTests
{
    public class EnergyDataServiceClientTests
    {
        [Fact]
        public void GetPricesShouldThrow()
        {
            var cut = CreateTestObject();

            var act = () => cut.GetPrices(PriceArea.DK2, DateTime.Now, DateTime.Now);

            act.Should().Throw<Exception>();
        }

        private static IEnergyDataServiceClient CreateTestObject()
        {
            return new EnergyDataServiceClient(Substitute.For<IRestClientFactory>());
        }
    }
}