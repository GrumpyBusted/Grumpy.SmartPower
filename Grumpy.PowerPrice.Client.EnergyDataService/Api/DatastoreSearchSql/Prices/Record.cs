using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DatastoreSearchSql.Prices
{
    internal class Record
    {
        [JsonPropertyName("HourDK")]
        public DateTime Hour { get; set; }
        [JsonPropertyName("SpotPriceDKK")]
        public double SpotPriceDKK { get; set; } = 0;
        [JsonPropertyName("SpotPriceEUR")]
        public double SpotPriceEUR { get; set; } = 0;
    }
}
