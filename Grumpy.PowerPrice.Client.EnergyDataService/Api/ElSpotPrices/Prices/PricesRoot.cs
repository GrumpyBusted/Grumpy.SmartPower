using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.Prices
{
    public class PricesRoot
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; } = new Data();
    }
}