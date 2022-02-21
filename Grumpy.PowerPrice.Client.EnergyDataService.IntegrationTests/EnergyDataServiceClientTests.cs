using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq;
using Grumpy.SmartPower.Core.Dto;
using Xunit;

namespace Grumpy.PowerPrice.Client.EnergyDataService.IntegrationTests;

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
        res.First().SpotPriceEUR.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetExchangeRateShouldGetValue()
    {
        var cut = CreateTestObject();

        var res = cut.GetExchangeRate(PriceArea.DK2, DateTime.Now);

        res.Should().BeInRange(700, 800);
    }

    [Fact]
    public void GetExchangeRateWherePriceIsNullShouldGetValue()
    {
        var cut = CreateTestObject();

        var res = cut.GetExchangeRate(PriceArea.DK2, DateTime.Parse("2022-02-20T16:00:00"));

        res.Should().BeInRange(700, 800);
    }

    private static IEnergyDataServiceClient CreateTestObject()
    {
        return new EnergyDataServiceClient(new RestClientFactory(Substitute.For<ILoggerFactory>()));
    }
}