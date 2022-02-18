using FluentAssertions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using NSubstitute;
using System;
using System.Collections.Generic;
using Grumpy.SmartPower.Core.Dto;
using Xunit;

namespace Grumpy.SmartPower.Infrastructure.UnitTests;

public class PowerPriceServiceTests
{
    [Fact]
    public void IsBatteryFullWith100ShouldBeTrue()
    {
        var client = Substitute.For<IEnergyDataServiceClient>();
        client.GetPrices(Arg.Is(PriceArea.DK2), Arg.Is(DateTime.Parse("2022-02-01T00:00:00")), Arg.Is(DateTime.Parse("2022-02-01T23:59:59"))).Returns(_ => new List<PowerPrice.Client.EnergyDataService.Dto.PowerPrice> { new() });

        var cut = new PowerPriceService(client);

        var res = cut.GetPrices(PriceArea.DK2, DateTime.Parse("2022-02-01T00:00:00"), DateTime.Parse("2022-02-01T23:59:59"));

        res.Should().HaveCount(1);
    }
}