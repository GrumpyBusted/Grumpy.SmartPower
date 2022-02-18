namespace Grumpy.Rest.Interface;

public interface IRestClientFactory
{
    public IRestClient Instance(string baseUrl);
}