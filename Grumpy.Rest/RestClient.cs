using Grumpy.Rest.Exceptions;
using Grumpy.Rest.Interface;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Grumpy.Rest;

public class RestClient : IRestClient
{
    private bool _disposed;
    private readonly ILogger<RestClient> _logger;
    private readonly RestSharp.RestClient _client;

    internal RestClient(string baseUrl, ILogger<RestClient> logger)
    {
        _logger = logger;

        var options = new RestClientOptions(baseUrl)
        {
            ThrowOnAnyError = true,
            ThrowOnDeserializationError = true
        };

        _client = new RestSharp.RestClient(options);
    }

    public T Execute<T>(RestRequest request)
    {
        _logger.LogInformation("Executing REST Web Service {0}", request);

        var response = _client.ExecuteAsync<T>(request);

        _logger.LogInformation("Response from REST Web Service {0}", response.Result);

        if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new RestClientExecuteException(request, response.Result);

        return response.Result.Data ?? throw new RestClientExecuteException(request, response.Result);
    }

    public void Execute(RestRequest request)
    {
        _logger.LogInformation("Executing REST Web Service {0}", request);

        var response = _client.ExecuteAsync(request);

        _logger.LogInformation("Response from REST Web Service {0}", response.Result);

        if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new RestClientExecuteException(request, response.Result);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) 
            return;
            
        if (disposing)
            _client.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}