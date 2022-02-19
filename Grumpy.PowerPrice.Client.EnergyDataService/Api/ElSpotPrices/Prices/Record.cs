using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.Prices
{
    public class Record
    {
        [JsonPropertyName("HourDK")]
        public DateTime Hour { get; set; }
        [JsonPropertyName("SpotPriceDKK")]
        public double SpotPriceDKK { get; set; }
        [JsonPropertyName("SpotPriceEUR")]
        public double SpotPriceEUR { get; set; }
    }
}