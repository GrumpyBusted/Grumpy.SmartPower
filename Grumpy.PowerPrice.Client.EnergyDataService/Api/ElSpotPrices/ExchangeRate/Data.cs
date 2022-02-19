using System.Text.Json.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.ExchangeRate
{
    public class Data
    {
        [JsonPropertyName("elspotprices")]
        public List<Record> Records { get; set; } = new List<Record>();
    }
}