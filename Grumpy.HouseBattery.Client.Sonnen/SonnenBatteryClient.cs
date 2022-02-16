using Grumpy.HouseBattery.Client.Sonnen.Dtos;
using Grumpy.HouseBattery.Client.Sonnen.Helpers;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.Json;
using Grumpy.Rest.Interface;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Text.RegularExpressions;

namespace Grumpy.HouseBattery.Client.Sonnen
{
    public class SonnenBatteryClient : ISonnenBatteryClient
    {
        private readonly SonnenBatteryClientOptions _options;
        private readonly IRestClientFactory _restClientFactory;

        public SonnenBatteryClient(IOptions<SonnenBatteryClientOptions> options, IRestClientFactory restClientFactory)
        {
            _options = options.Value;
            _restClientFactory = restClientFactory;
        }

        public int GetProduction()
        {
            return GetStatus().Production;
        }

        public int GetConsumption()
        {
            return GetStatus().Consumption;
        }

        public int GetBatterySize()
        {
            return GetLatestData().FullChargeCapacity;
        }

        public int GetBatteryCapacity()
        {
            return GetStatus().RemainingCapacity;
        }

        public int GetBatteryLevel()
        {
            return GetStatus().UserStateOfCharge;
        }

        public OperatingMode GetOperatingMode()
        {
            using var client = CreateClient();

            var request = CreateRequest("configurations/EM_OperatingMode", Method.Get);

            var response = client.Execute<Api.Configurations.OperatingMode.Root>(request);

            return OperatingModeHelper.Parse(response.OperatingMode);
        }

        public IEnumerable<TimeOfUseEvent> GetSchedule()
        {
            using var client = CreateClient();

            var request = CreateRequest("configurations/EM_ToU_Schedule", Method.Get);

            var response = client.Execute<Api.Configurations.TimeOfUseSchedule.Root>(request);

            return MapFromSonnenTimeOfUseSchedule(response.TimeOfUseSchedule);
        }

        public void SetOperatingMode(OperatingMode operationMode)
        {
            var body = new Api.Configurations.OperatingMode.Root()
            {
                OperatingMode = operationMode.ToApiString()
            };

            using var client = CreateClient();

            var request = CreateRequest("configurations", Method.Put)
                .AddBody(body);

            client.Execute(request);
        }

        public void SetSchedule(IEnumerable<TimeOfUseEvent> schedule)
        {
            ValidateSchedule(schedule);

            var body = MapToSonnenTimeOfUseSchedule(schedule);

            using var client = CreateClient();

            var request = CreateRequest("configurations", Method.Put)
                .AddBody(body);

            client.Execute(request);
        }

        private Api.Status.Root GetStatus()
        {
            using var client = CreateClient();

            var request = CreateRequest("status", Method.Get);

            return client.Execute<Api.Status.Root>(request);
        }

        private Api.LatestData.Root GetLatestData()
        {
            using var client = CreateClient();

            var request = CreateRequest("latestdata", Method.Get);

            return client.Execute<Api.LatestData.Root>(request);
        }

        private IRestClient CreateClient()
        {
            return _restClientFactory.Instance($"http://{_options.Ip}:80/api/v2");
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);

            request.AddHeader("Auth-Token", _options.ApiToken);

            return request;
        }

        private static void ValidateSchedule(IEnumerable<TimeOfUseEvent> schedule)
        {
            foreach (var item in schedule)
            {
                if (!Regex.Match(item.Start, "^([0-1][0-9]|[2][0-3]):([0-5][0-9])$").Success)
                    throw new ArgumentException("Invalid start time", nameof(schedule));
                if (!Regex.Match(item.End, "^([0-1][0-9]|[2][0-3]):([0-5][0-9])$").Success)
                    throw new ArgumentException("Invalid stop time", nameof(schedule));
                if (item.Watt < 0)
                    throw new ArgumentException("Invalid max watt", nameof(schedule));
            }
        }

        private static Api.Configurations.TimeOfUseSchedule.Root MapToSonnenTimeOfUseSchedule(IEnumerable<TimeOfUseEvent> schedule)
        {
            var list = new List<Api.Configurations.TimeOfUseSchedule.Schedule>();

            foreach (var item in schedule)
            {
                list.Add(new Api.Configurations.TimeOfUseSchedule.Schedule()
                {
                    Start = item.Start,
                    Stop = item.End,
                    Max = item.Watt
                });
            }

            var res = new Api.Configurations.TimeOfUseSchedule.Root()
            {
                TimeOfUseSchedule = list.SerializeToJson()
            };

            return res;
        }

        private static IEnumerable<TimeOfUseEvent> MapFromSonnenTimeOfUseSchedule(string timeOfUseSchedule)
        {
            var list = timeOfUseSchedule.DeserializeFromJson<List<Api.Configurations.TimeOfUseSchedule.Schedule>>();

            foreach (var item in list)
            {
                yield return new TimeOfUseEvent()
                {
                    Start = item.Start,
                    End = item.Stop,
                    Watt = item.Max
                };
            }
        }
    }
}