using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.DatastoreSearchSql.ExchangeRate
{
    public class Result
    {
        [JsonPropertyName("records")]
        public List<Record> Records { get; set; } = new List<Record>();
    }
}
