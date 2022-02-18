using FluentAssertions;
using Grumpy.Rest.Interface;
using Grumpy.Weather.Client.VisualCrossing.Api.Timeline;
using Grumpy.Weather.Client.VisualCrossing.Interface;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grumpy.Weather.Client.VisualCrossing.UnitTests;

public class VisualCrossingWeatherClientTests
{
    private readonly IRestClientFactory _restClientFactory = Substitute.For<IRestClientFactory>();
    private readonly IRestClient _restClient = Substitute.For<IRestClient>();

    public VisualCrossingWeatherClientTests()
    {
        _restClientFactory.Instance(Arg.Any<string>()).Returns(_restClient);
    }

    [Fact]
    public void GetShouldReturnFromRestRequest()
    {
        var exp = new Root
        {
            Days = new List<Day>
            {
                new()
                {
                    Hours = new List<Hour>
                    {
                        new()
                        {
                            Temperature = 2
                        }
                    }
                }
            }
        };
        _restClient.Execute<Root>(Arg.Any<RestRequest>()).Returns(exp);
        var cut = CreateTestObject();

        var res = cut.Get(DateOnly.Parse("2022-02-13")).ToList();

        res.Should().HaveCount(1);
        res.First().Temperature.Should().Be(2);
    }

    private IVisualCrossingWeatherClient CreateTestObject()
    {
        return new VisualCrossingWeatherClient(Options.Create(new VisualCrossingWeatherClientOptions()), _restClientFactory);
    }
}