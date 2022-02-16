using System.Runtime.Serialization;

namespace Grumpy.PowerMeter.Client.SmartMe.Exceptions
{
    [Serializable]
    internal class InvalidDeviceException : Exception
    {
        private const string Text = "Invalid device";

        public InvalidDeviceException() : base(Text) { }

        public InvalidDeviceException(string message) : base(message) { }

        public InvalidDeviceException(string message, Exception inner) : base(message, inner) { }

        protected InvalidDeviceException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}