using System.Runtime.Serialization;

namespace Grumpy.PowerMeter.Client.SmartMe.Exceptions
{
    [Serializable]
    internal class InvalidMeterValueException : Exception
    {
        private const string Text = "Invalid meter value reading";
        private readonly string? _unit;
        private readonly double? _value;

        public InvalidMeterValueException(string unit, double value) : base(Text)
        {
            _unit = unit;
            _value = value;
        }

        public InvalidMeterValueException() : base(Text) { }

        public InvalidMeterValueException(string message) : base(message) { }

        public InvalidMeterValueException(string message, Exception inner) : base(message, inner) { }

        protected InvalidMeterValueException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_unit), _unit);
            info.AddValue(nameof(_value), _value);

            base.GetObjectData(info, context);
        }
    }
}