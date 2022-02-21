using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.ExchangeRate;

public class ExchangeRateRoot
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}