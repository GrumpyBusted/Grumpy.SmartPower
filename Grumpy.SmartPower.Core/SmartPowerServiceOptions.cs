using Grumpy.SmartPower.Core.Dto;

namespace Grumpy.SmartPower.Core;

public class SmartPowerServiceOptions
{
    public PriceArea PriceArea { get; set; } = PriceArea.DK1;
    public PriceArea FallBackPriceArea { get; set; } = PriceArea.DK;
}