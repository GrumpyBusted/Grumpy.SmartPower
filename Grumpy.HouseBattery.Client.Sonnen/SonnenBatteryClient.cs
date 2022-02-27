using Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.OperatingMode;
using Grumpy.HouseBattery.Client.Sonnen.Api.Configurations.TimeOfUseSchedule;
using Grumpy.HouseBattery.Client.Sonnen.Api.LatestData;
using Grumpy.HouseBattery.Client.Sonnen.Api.Status;
using Grumpy.HouseBattery.Client.Sonnen.Helpers;
using Grumpy.HouseBattery.Client.Sonnen.Interface;
using Grumpy.Json;
using Grumpy.Rest.Interface;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Grumpy.HouseBattery.Client.Sonnen.Dto;

[assembly: InternalsVisibleTo("Grumpy.HouseBattery.Client.Sonnen.UnitTests")]

namespace Grumpy.HouseBattery.Client.Sonnen;

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
        // Data from battery API not correct
        return _options.BatterySize > 0 ? _options.BatterySize : GetLatestData().FullChargeCapacity;
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

        var response = client.Execute<OperatingModeRoot>(request);

        return OperatingModeHelper.Parse(response.OperatingMode);
    }

    public IEnumerable<TimeOfUseEvent> GetSchedule()
    {
        using var client = CreateClient();

        var request = CreateRequest("configurations/EM_ToU_Schedule", Method.Get);

        var response = client.Execute<TimeOfUseScheduleRoot>(request);

        return MapFromSonnenTimeOfUseSchedule(response.TimeOfUseSchedule);
    }

    public void SetOperatingMode(OperatingMode operationMode)
    {
        var body = new OperatingModeRoot
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
        var list = schedule.ToList();

        ValidateSchedule(list);

        var body = MapToSonnenTimeOfUseSchedule(list);

        using var client = CreateClient();

        var request = CreateRequest("configurations", Method.Put)
            .AddBody(body);

        client.Execute(request);
    }

    public int InverterLimit()
    {
        return _options.InverterLimit;
    }

    private StatusRoot GetStatus()
    {
        using var client = CreateClient();

        var request = CreateRequest("status", Method.Get);

        return client.Execute<StatusRoot>(request);
    }

    private LatestDataRoot GetLatestData()
    {
        using var client = CreateClient();

        // ReSharper disable once StringLiteralTypo
        var request = CreateRequest("latestdata", Method.Get);

        return client.Execute<LatestDataRoot>(request);
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

    private static TimeOfUseScheduleRoot MapToSonnenTimeOfUseSchedule(IEnumerable<TimeOfUseEvent> schedule)
    {
        var list = schedule.Select(item => new Schedule
        {
            Start = item.Start,
            Stop = item.End,
            Max = item.Watt
        }).ToList();

        var res = new TimeOfUseScheduleRoot
        {
            TimeOfUseSchedule = list.SerializeToJson()
        };

        return res;
    }

    private static IEnumerable<TimeOfUseEvent> MapFromSonnenTimeOfUseSchedule(string timeOfUseSchedule)
    {
        var list = timeOfUseSchedule.DeserializeFromJson<List<Schedule>>();

        foreach (var item in list)
        {
            yield return new TimeOfUseEvent
            {
                Start = item.Start,
                End = item.Stop,
                Watt = item.Max
            };
        }
    }

    public int GetGridFeedIn()
    {
        return GetStatus().GridFeedIn;
    }
}