using Grumpy.PowerPrice.Client.EnergyDataService.Api.ElSpotPrices.Prices;
using System.Runtime.Serialization;

namespace Grumpy.PowerPrice.Client.EnergyDataService.Exceptions
{
    [Serializable]
    internal class EnergyDataServiceException : Exception
    {
        private const string Text = "Energy Data Service Exception";
        private readonly PricesRoot? _response;

        public EnergyDataServiceException(PricesRoot response) : base(Text)
        {
            _response = response;
        }

        public EnergyDataServiceException() : base(Text) { }

        public EnergyDataServiceException(string message) : base(message) { }

        public EnergyDataServiceException(string message, Exception inner) : base(message, inner) { }

        protected EnergyDataServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_response), _response);

            base.GetObjectData(info, context);
        }
    }
}