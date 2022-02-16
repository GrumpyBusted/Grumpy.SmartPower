using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DatastoreSearchSql.Prices
{
    internal class PricesRoot
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; } = false;
        [JsonPropertyName("result")]
        public Result Result { get; set; } = new Result();
    }
}
