using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DataStoreSearchSql.ExchangeRate;

internal class ExchangeRateRoot
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;
    [JsonPropertyName("result")]
    public Result Result { get; set; } = new();
}