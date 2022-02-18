using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DataStoreSearchSql.Prices;

internal class Result
{
    [JsonPropertyName("records")]
    public List<Record> Records { get; set; } = new();
}