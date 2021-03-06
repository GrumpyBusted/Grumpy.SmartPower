using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.Rest.Interface;
using RestSharp;
using System.Runtime.CompilerServices;
using Grumpy.SmartPower.Core.Dto;
using Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.ExchangeRate;
using Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.Prices;
using Grumpy.PowerPrice.Client.EnergyDataService.Exceptions;

[assembly: InternalsVisibleTo("Grumpy.PowerPrice.Client.EnergyDataService.UnitTests")]

namespace Grumpy.PowerPrice.Client.EnergyDataService;

public class EnergyDataServiceClient : IEnergyDataServiceClient
{
    private readonly IRestClientFactory _restClientFactory;

    public EnergyDataServiceClient(IRestClientFactory restClientFactory)
    {
        _restClientFactory = restClientFactory;
    }

    public IEnumerable<Dto.PowerPrice> GetPrices(PriceArea priceArea, DateTime from, DateTime to)
    {
        var client = CreateClient();

        var request = CreateRequest("query { elspotprices(where: { HourDK: { _gte: \"" + from.ToString("yyyy-MM-ddTHH:mm:ss.000Z") + "\", _lte: \"" + to.ToString("yyyy-MM-ddTHH:mm:ss.000Z") + "\"}, PriceArea: { _eq: \"" + priceArea + "\" }}) { HourDK SpotPriceDKK SpotPriceEUR }}");

        var response = client.Execute<PricesRoot>(request);

        if (response.Data.Records == null)
            throw new EnergyDataServiceException(response);

        return MapToPriceList(response.Data.Records);
    }

    public double GetExchangeRate(PriceArea priceArea, DateTime dateTime)
    {
        var client = CreateClient();

        var request = CreateRequest("query { elspotprices(where: { HourDK: { _lte: \"" + dateTime.ToString("yyyy-MM-ddTHH:mm:ss.000Z") + "\"}, PriceArea: { _eq: \"" + priceArea + "\"}, SpotPriceEUR: { _gt: 0}, SpotPriceDKK: { _gt: 0} } order_by: { HourDK: desc}, limit: 1) { SpotPriceDKK SpotPriceEUR }}");

        var response = client.Execute<ExchangeRateRoot>(request);

        var record = response.Data.Records.FirstOrDefault();

        return record == null ? 765 : record.SpotPriceDKK / record.SpotPriceEUR * 100;
    }

    private static RestRequest CreateRequest(string query)
    {
        return new RestRequest("v1/graphql", Method.Post)
            .AddJsonBody(new { query });
    }

    private static IEnumerable<Dto.PowerPrice> MapToPriceList(IEnumerable<Api.ElSpotPrices.Prices.Record> records)
    {
        return records.Select(record => new Dto.PowerPrice
        {
            Hour = record.Hour,
            SpotPriceDKK = record.SpotPriceDKK,
            SpotPriceEUR = record.SpotPriceEUR
        });
    }

    private IRestClient CreateClient()
    {
        return _restClientFactory.Instance("https://data-api.energidataservice.dk");
    }
}