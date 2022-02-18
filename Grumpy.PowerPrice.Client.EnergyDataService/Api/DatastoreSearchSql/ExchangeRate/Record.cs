using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DataStoreSearchSql.ExchangeRate;

internal class Record
{
    [JsonPropertyName("SpotPriceDKK")]
    public double SpotPriceDKK { get; set; } = 0;
    [JsonPropertyName("SpotPriceEUR")]
    public double SpotPriceEUR { get; set; } = 0;
}