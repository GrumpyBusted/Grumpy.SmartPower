using RestSharp;
using System.Runtime.Serialization;

namespace Grumpy.Rest.Exceptions
{
    [Serializable]
    internal class RestClientExecuteException : Exception
    {
        private const string Text = "REST Client Execute Exception";
        private readonly RestRequest? _request;
        private readonly RestResponse? _response;

        public RestClientExecuteException(RestRequest request, RestResponse response) : base(Text)
        {
            _request = request;
            _response = response;
        }

        public RestClientExecuteException() : base(Text) { }

        public RestClientExecuteException(string message) : base(message) { }

        public RestClientExecuteException(string message, Exception innerException) : base(message, innerException) { }

        protected RestClientExecuteException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_request), _request);
            info.AddValue(nameof(_response), _response);

            base.GetObjectData(info, context);
        }
    }
}