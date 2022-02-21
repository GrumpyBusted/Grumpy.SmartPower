using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.ExchangeRate;

public class Record
{
    [JsonPropertyName("SpotPriceDKK")]
    public double SpotPriceDKK { get; set; }
    [JsonPropertyName("SpotPriceEUR")]
    public double SpotPriceEUR { get; set; }
}