using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Options;
using Grumpy.Caching.Interface;

namespace Grumpy.SmartPower.Infrastructure;

public class RealTimeReadingRepository : IRealTimeReadingRepository
{
    private readonly RealTimeReadingRepositoryOptions _options;
    private readonly ICache _fileCache;

    public RealTimeReadingRepository(IOptions<RealTimeReadingRepositoryOptions> options, ICacheFactory cacheFactory)
    {
        _options = options.Value;
        _fileCache = cacheFactory.FileCacheInstance(GetType().FullName ?? nameof(RealTimeReadingRepository));
    }

    public void Save(DateTime dateTime, int consumption, int production, int gridFeedIn)
    {
        var record = new RealTimeReading
        {
            DateTime = new DateTime(dateTime.Ticks, dateTime.Kind == DateTimeKind.Unspecified ? DateTimeKind.Local : dateTime.Kind),
            Consumption = consumption,
            Production = production,
            GridFeedIn = gridFeedIn < 0 ? Math.Abs(gridFeedIn) : 0,
            GridFeedOut = gridFeedIn > 0 ? gridFeedIn : 0
        };

        var folder = Path.GetDirectoryName(_options.RepositoryPath) ?? ".";

        if (folder != "" && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        if (!File.Exists(_options.RepositoryPath))
            File.WriteAllText(_options.RepositoryPath, record.CsvHeader(';') + Environment.NewLine);

        File.AppendAllText(_options.RepositoryPath, record.CsvRecord(';') + Environment.NewLine);
    }

    public int? GetConsumption(DateTime hour)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:Consumption:{hour}", TimeSpan.FromDays(365),
            () => GetValues(hour).Consumption);
    }

    public int? GetProduction(DateTime hour)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:Production:{hour}", TimeSpan.FromDays(365),
            () => GetValues(hour).Production);
    }

    public int? GetGridFeedIn(DateTime hour)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:GridFeedIn:{hour}", TimeSpan.FromDays(365),
            () => GetValues(hour).GridFeedIn);
    }

    public int? GetGridFeedOut(DateTime hour)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:GridFeedOut:{hour}", TimeSpan.FromDays(365),
            () => GetValues(hour).GridFeedOut);
    }

    private RealTimeReading GetValues(DateTime hour)
    {
        var from = new DateTime(hour.Year, hour.Month, hour.Day, hour.Hour, 0, 0, hour.Kind);
        var to = from.AddHours(1).AddMilliseconds(-1);

        if (to > DateTime.Now)
            throw new ArgumentException("Only ask for passed readings", nameof(hour));

        var list = ReadFrom(_options.RepositoryPath).Where(r => r.DateTime >= from && r.DateTime <= to).ToList();

        return new RealTimeReading
        {
            DateTime = hour,
            Consumption = list.Count == 0 ? null : list.Sum(r => r.Consumption) / list.Count,
            Production = list.Count == 0 ? null : list.Sum(r => r.Production) / list.Count,
            GridFeedIn = list.Count == 0 ? null : list.Sum(r => r.GridFeedIn) / list.Count,
            GridFeedOut = list.Count == 0 ? null : list.Sum(r => r.GridFeedOut) / list.Count
        };
    }

    private static IEnumerable<RealTimeReading> ReadFrom(string file)
    {
        if (!File.Exists(file))
            yield break;

        using var reader = File.OpenText(file);

        for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
            if (line.IsCsvHeader<RealTimeReading>(';')) 
                continue;

            yield return line.ParseCsv<RealTimeReading>(';');
        }
    }
}