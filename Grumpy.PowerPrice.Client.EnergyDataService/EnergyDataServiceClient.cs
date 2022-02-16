using Grumpy.PowerPrice.Client.EnergyDataService.Exceptions;
using Grumpy.PowerPrice.Client.EnergyDataService.Interface;
using Grumpy.Rest.Interface;
using Grumpy.SmartPower.Core.Dtos;
using RestSharp;

namespace Grumpy.PowerPrice.Client.EnergyDataService
{
    public class EnergyDataServiceClient : IEnergyDataServiceClient
    {
        private readonly IRestClientFactory _restClientFactory;

        public EnergyDataServiceClient(IRestClientFactory restClientFactory)
        {
            _restClientFactory = restClientFactory;
        }

        public IEnumerable<Dtos.PowerPrice> GetPrices(PriceArea priceArea, DateTime from, DateTime to)
        {
            var client = CreateClient();

            var request = CreateRequest($"SELECT \"HourDK\", \"SpotPriceDKK\", \"SpotPriceEUR\" FROM ELSpotPrices WHERE \"HourDK\" BETWEEN TIMESTAMP '{from:yyyy-MM-dd HH:mm:ss}' AND TIMESTAMP '{to:yyyy-MM-dd HH:mm:ss}' AND \"PriceArea\" = '{priceArea}'");

            var response = client.Execute<Api.DatastoreSearchSql.Prices.Root>(request);

            if (!response.Success || response.Result.Records == null)
                throw new EnergyDataServiceException(response);

            return MapToPriceList(response.Result.Records, priceArea, from);
        }

        private static RestRequest CreateRequest(string query)
        {
            return new RestRequest("datastore_search_sql", Method.Get)
                .AddParameter("sql", query);
        }

        private IEnumerable<Dtos.PowerPrice> MapToPriceList(IEnumerable<Api.DatastoreSearchSql.Prices.Record> records, PriceArea priceArea, DateTime from)
        {
            var rate = new Lazy<double>(() => GetExchangeRate(priceArea, from));

            foreach (var record in records)
            {
                var powerPrice = new Dtos.PowerPrice()
                {
                    Hour = record.Hour,
                    Price = record.SpotPriceDKK
                };

                if (powerPrice.Price == 0)
                    powerPrice.Price = record.SpotPriceEUR * rate.Value;

                yield return powerPrice;
            }
        }

        private double GetExchangeRate(PriceArea priceArea, DateTime dateTime)
        {
            var client = CreateClient();

            var request = CreateRequest($"SELECT \"SpotPriceDKK\", \"SpotPriceEUR\" FROM ELSpotPrices WHERE \"SpotPriceDKK\" IS NOT NULL AND \"SpotPriceEUR\" IS NOT NULL AND \"HourDK\" <= TIMESTAMP '{dateTime:yyyy-MM-dd HH:mm:ss}' AND \"PriceArea\" = '{priceArea}' ORDER BY \"HourDK\" DESC FETCH FIRST 1 ROWS ONLY");

            var response = client.Execute<Api.DatastoreSearchSql.ExchangeRate.Root>(request);

            var record = response.Result.Records.FirstOrDefault();

            return record == null ? 7.65 : record.SpotPriceDKK / record.SpotPriceEUR;
        }

        private IRestClient CreateClient()
        {
            return _restClientFactory.Instance("https://api.energidataservice.dk");
        }
    }
}