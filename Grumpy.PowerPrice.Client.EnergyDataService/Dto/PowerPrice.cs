namespace Grumpy.PowerPrice.Client.EnergyDataService.Dto
{
    public class PowerPrice
    {
        public DateTime Hour { get; set; } = DateTime.MinValue;
        public double? SpotPriceDKK { get; set; } = 0;
        public double SpotPriceEUR { get; set; } = 0;
    }
}