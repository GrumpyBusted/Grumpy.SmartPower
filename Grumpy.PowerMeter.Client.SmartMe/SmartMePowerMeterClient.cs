using Grumpy.PowerMeter.Client.SmartMe.Api.DeviceBySerial;
using Grumpy.PowerMeter.Client.SmartMe.Api.MeterValues;
using Grumpy.PowerMeter.Client.SmartMe.Exceptions;
using Grumpy.PowerMeter.Client.SmartMe.Interface;
using Grumpy.Rest.Interface;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Grumpy.PowerMeter.Client.SmartMe.UnitTests")]

namespace Grumpy.PowerMeter.Client.SmartMe
{

    public class SmartMePowerMeterClient : ISmartMePowerMeterClient
    {
        private readonly SmartMePowerMeterClientOptions _options;
        private readonly IRestClientFactory _restClientFactory;
        private readonly Lazy<string> _id;

        public SmartMePowerMeterClient(IOptions<SmartMePowerMeterClientOptions> options, IRestClientFactory restClientFactory )
        {
            _options = options.Value;
            _restClientFactory = restClientFactory;
            _id = new Lazy<string>(() => GetId(_options.SerialNo));
        }

        public double GetValue(DateTime dateTime)
        {
            using var client = CreateClient();

            var request = CreateRequest($"MeterValues/{_id.Value}", Method.Get)
                .AddQueryParameter("date", dateTime.ToString("yyyy-MM-ddTHH:mm:ss"));

            var response = client.Execute<MeterValuesRoot>(request);

            if (response.CounterReadingUnit != "kWh")
                throw new InvalidMeterValueException(response.CounterReadingUnit, response.CounterReading);

            if (response.CounterReading < 0.1)
                throw new InvalidMeterValueException(response.CounterReadingUnit, response.CounterReading);

            return response.CounterReading;
        }

        private string GetId(int serialNo)
        {
            using var client = CreateClient();

            var request = CreateRequest("DeviceBySerial", Method.Get)
                .AddQueryParameter("serial", serialNo);

            var response = client.Execute<DeviceBySerialRoot>(request);

            return response.Id;
        }

        private IRestClient CreateClient()
        {
            return _restClientFactory.Instance("https://smart-me.com:443/api");
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method)
                .AddHeader("Authorization", $"Basic {_options.ApiToken}");

            return request;
        }
    }
}