using RestSharp;

namespace Grumpy.Rest.Interface
{
    public interface IRestClient : IDisposable
    {
        public T Execute<T>(RestRequest request);
        public void Execute(RestRequest request);
    }
}