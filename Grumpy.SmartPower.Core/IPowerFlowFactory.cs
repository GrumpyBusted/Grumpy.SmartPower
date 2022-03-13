using Grumpy.SmartPower.Core.Consumption;
using Grumpy.SmartPower.Core.Model;
using Grumpy.SmartPower.Core.Production;

namespace Grumpy.SmartPower.Core
{
    public interface IPowerFlowFactory
    {
        public IPowerFlow Instance();
    }
}