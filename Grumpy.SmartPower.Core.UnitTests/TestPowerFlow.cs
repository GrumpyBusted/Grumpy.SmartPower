using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grumpy.SmartPower.Core.UnitTests
{
    public class TestPowerFlow
    {
        private readonly IList<ProductionItem> _productions = new List<ProductionItem>();
        private readonly IList<ConsumptionItem> _consumptions = new List<ConsumptionItem>();
        private readonly IList<PriceItem> _prices = new List<PriceItem>();
        private DateTime _hour;

        public TestPowerFlow(string hour)
        {
            _hour = DateTime.Parse(hour).ToHour();
        }

        public void Add(int? production, int? consumption, double? price)
        {
            if (production != null)
            {
                _productions.Add(new ProductionItem
                {
                    Hour = _hour,
                    WattPerHour = production ?? 0
                });
            }

            if (consumption != null)
            {
                _consumptions.Add(new ConsumptionItem
                {
                    Hour = _hour,
                    WattPerHour = consumption ?? 0
                });
            }

            if (price != null)
            {
                _prices.Add(new PriceItem
                {
                    Hour = _hour,
                    Price = price ?? 0
                });
            }

            _hour = _hour.AddHours(1);
        }

        public IEnumerable<ProductionItem> Productions => _productions;

        public IEnumerable<ConsumptionItem> Consumptions => _consumptions;

        public IEnumerable<PriceItem> Prices => _prices;

        public DateTime Start => _productions.Count == 0 ? _hour : _productions.Min(i => i.Hour);

        public DateTime End => _hour;
    }
}
