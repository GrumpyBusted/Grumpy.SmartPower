using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DatastoreSearchSql.ExchangeRate
{
    internal class Result
    {
        [JsonPropertyName("records")]
        public List<Record> Records { get; set; } = new List<Record>();
    }
}
