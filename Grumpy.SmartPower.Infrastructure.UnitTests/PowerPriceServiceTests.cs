using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using NSubstitute;
using System;
using System.Collections.Generic;
using Grumpy.SmartPower.Core.Dto;
using Xunit;
using System.Linq;
using Grumpy.Caching.TestMocks;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class PowerPriceServiceTests
{
    [Fact]
    public void GetPricesShouldReturnListFromClient()
    {
        var client = Substitute.For<IEnergyDataServiceClient>();
        client.GetPrices(Arg.Is(PriceArea.DK2), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> { 
            new()
            {
                Hour = DateTime.Parse("2022-02-01T00:00:00"),
                SpotPriceDKK = 1
            }
        });

        var cut = new PowerPriceService(client, TestCacheFactory.Instance);

        var res = cut.GetPrices(PriceArea.DK2, PriceArea.DK, DateTime.Parse("2022-02-01T00:00:00"), DateTime.Parse("2022-02-01T00:59:59"));

        res.Should().HaveCount(1);
    }

    [Fact]
    public void GetPricesWithMissingDKKPriceShouldReturnExchangedPrice()
    {
        var client = Substitute.For<IEnergyDataServiceClient>();
        client.GetPrices(Arg.Is(PriceArea.DK2), Arg.Is(DateTime.Parse("2022-03-01T00:00:00")), Arg.Is(DateTime.Parse("2022-03-01T00:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> { 
            new() 
            {
                Hour = DateTime.Parse("2022-03-01T00:00:00"),
                SpotPriceDKK = null,
                SpotPriceEUR = 200
            }
        });
        client.GetExchangeRate(Arg.Is(PriceArea.DK2), Arg.Is(DateTime.Parse("2022-03-01T00:00:00"))).Returns(750);

        var cut = new PowerPriceService(client, TestCacheFactory.Instance);

        var res = cut.GetPrices(PriceArea.DK2, PriceArea.DK, DateTime.Parse("2022-03-01T00:00:00"), DateTime.Parse("2022-03-01T00:59:59"));

        res.First().Price.Should().Be(1500);
    }

    [Fact]
    public void GetPricesMissingHourShouldFallBackToDK1()
    {
        var client = Substitute.For<IEnergyDataServiceClient>();
        client.GetPrices(Arg.Is(PriceArea.DK2), Arg.Is(DateTime.Parse("2022-03-01T00:00:00")), Arg.Is(DateTime.Parse("2022-03-01T01:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> {
            new()
            {
                Hour = DateTime.Parse("2022-03-01T00:00:00"),
                SpotPriceDKK = 600
            }
        });
        client.GetPrices(Arg.Is(PriceArea.DK), Arg.Is(DateTime.Parse("2022-03-01T00:00:00")), Arg.Is(DateTime.Parse("2022-03-01T01:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> {
            new()
            {
                Hour = DateTime.Parse("2022-03-01T01:00:00"),
                SpotPriceDKK = 700
            }
        });

        var cut = new PowerPriceService(client, TestCacheFactory.Instance);

        var res = cut.GetPrices(PriceArea.DK2, PriceArea.DK, DateTime.Parse("2022-03-01T00:00:00"), DateTime.Parse("2022-03-01T01:59:59")).ToList();

        res.First(p => p.Hour == DateTime.Parse("2022-03-01T00:00:00")).Price.Should().Be(600);
        res.First(p => p.Hour == DateTime.Parse("2022-03-01T01:00:00")).Price.Should().Be(700);
    }

    [Fact]
    public void GetPricesShouldUseLastFallBack()
    {
        var client = Substitute.For<IEnergyDataServiceClient>();
        client.GetPrices(Arg.Is(PriceArea.DK2), Arg.Is(DateTime.Parse("2022-03-01T00:00:00")), Arg.Is(DateTime.Parse("2022-03-01T02:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice>());
        client.GetPrices(Arg.Is(PriceArea.DK), Arg.Is(DateTime.Parse("2022-03-01T00:00:00")), Arg.Is(DateTime.Parse("2022-03-01T02:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> {
            new()
            {
                Hour = DateTime.Parse("2022-03-01T01:00:00"),
                SpotPriceDKK = 700
            }
        });

        var cut = new PowerPriceService(client, TestCacheFactory.Instance);

        var res = cut.GetPrices(PriceArea.DK2, PriceArea.DK, DateTime.Parse("2022-03-01T00:00:00"), DateTime.Parse("2022-03-01T02:59:59")).ToList();

        res.First(p => p.Hour == DateTime.Parse("2022-03-01T00:00:00")).Price.Should().Be(700);
        res.First(p => p.Hour == DateTime.Parse("2022-03-01T01:00:00")).Price.Should().Be(700);
        res.First(p => p.Hour == DateTime.Parse("2022-03-01T02:00:00")).Price.Should().Be(700);
    }

    [Fact]
    public void GetPricesMissingHourWithOurFallBackShouldTakeLatest()
    {
        var client = Substitute.For<IEnergyDataServiceClient>();
        client.GetPrices(Arg.Is(PriceArea.DK2), Arg.Is(DateTime.Parse("2022-03-01T00:00:00")), Arg.Is(DateTime.Parse("2022-03-01T01:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> {
            new()
            {
                Hour = DateTime.Parse("2022-03-01T00:00:00"),
                SpotPriceDKK = 600
            }
        });

        var cut = new PowerPriceService(client, TestCacheFactory.Instance);

        var res = cut.GetPrices(PriceArea.DK2, PriceArea.DK, DateTime.Parse("2022-03-01T00:00:00"), DateTime.Parse("2022-03-01T01:59:59")).ToList();

        res.First(p => p.Hour == DateTime.Parse("2022-03-01T00:00:00")).Price.Should().Be(600);
        res.First(p => p.Hour == DateTime.Parse("2022-03-01T01:00:00")).Price.Should().Be(600);
    }
}