namespace Grumpy.PowerPrice.Client.EnergyDataService.Dto
{
    public class PowerPrice
    {
        public DateTime Hour { get; set; } = DateTime.MinValue;
        public double Price { get; set; } = 0;
    }
}