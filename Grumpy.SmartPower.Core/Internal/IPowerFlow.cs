using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;
using System.Collections;

namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlow : IEnumerator<PowerHour>, IEnumerable<PowerHour>
    {
        void Add(DateTime from, IEnumerable<ProductionItem> productions, IEnumerable<ConsumptionItem> consumptions, IEnumerable<PriceItem> prices);
        void Add(DateTime hour, int production, int consumption, double price);
        IEnumerable<PowerHour> All();
        PowerHour? Get(DateTime hour);
        PowerHour? First();
        PowerHour? Last();
        int Charge(DateTime hour, int? value = null);
        int Charge(PowerHour item, int? value = null);
        int Discharge(DateTime hour, int? value = null);
        int Discharge(PowerHour item, int? value = null);
        int Move(DateTime from, DateTime to, int? value = null);
        int Move(PowerHour source, PowerHour target, int? value = null);
    }
}