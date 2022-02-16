using System.Runtime.Serialization;

namespace Grumpy.Json.Exceptions
{
    [Serializable]
    internal class JsonDeserielizeException : Exception
    {
        private const string Text = "Unable to deserielize string to object";
        private readonly Type? _type;
        private readonly string? _value;

        public JsonDeserielizeException(Type type, string value) : base(Text)
        {
            _type = type;
            _value = value;
        }

        public JsonDeserielizeException() : base(Text) { }

        public JsonDeserielizeException(string message) : base(message) { }

        public JsonDeserielizeException(string message, Exception innerException) : base(message, innerException) { }

        protected JsonDeserielizeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_type), _type);
            info.AddValue(nameof(_value), _value);

            base.GetObjectData(info, context);
        }
    }
}