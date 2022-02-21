using Grumpy.Caching.Extensions;
using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Runtime.Caching;

namespace Grumpy.SmartPower.Infrastructure;

public class RealTimeReadingRepository : IRealTimeReadingRepository
{
    private readonly IOptions<RealTimeReadingRepositoryOptions> _options;
    private readonly FileCache _fileCache;

    public RealTimeReadingRepository(IOptions<RealTimeReadingRepositoryOptions> options)
    {
        _options = options;
        _fileCache = new FileCache(FileCacheManagers.Hashed);
    }

    public void Save(DateTime dateTime, int consumption, int production)
    {
        var record = new RealTimeReading
        {
            DateTime = dateTime,
            Consumption = consumption,
            Production = production
        };

        var folder = Path.GetDirectoryName(_options.Value.RepositoryPath) ?? ".";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        if (!File.Exists(_options.Value.RepositoryPath))
            File.WriteAllText(_options.Value.RepositoryPath, record.CsvHeader(';') + Environment.NewLine);

        File.AppendAllText(_options.Value.RepositoryPath, record.CsvRecord(';') + Environment.NewLine);
    }

    public int? GetConsumption(DateTime hour)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:Consumption:{hour}", TimeSpan.FromDays(365),
            () => GetValues(hour).Consumption);
    }

    public int? GetProduction(DateTime hour)
    {
        return _fileCache.TryGetIfNotSet($"{GetType().FullName}:Production:{hour}", TimeSpan.FromDays(365),
            () => GetValues(hour).Consumption);
    }

    private RealTimeReading GetValues(DateTime hour)
    {
        var from = new DateTime(hour.Year, hour.Month, hour.Day, hour.Hour, 0, 0, hour.Kind);
        var to = from.AddHours(1).AddMilliseconds(-1);

        if (to > DateTime.Now)
            throw new ArgumentException("Only ask for passed readings", nameof(hour));

        var list = ReadFrom(_options.Value.RepositoryPath).Where(r => r.DateTime >= from && r.DateTime <= to).ToList();

        return new RealTimeReading
        {
            DateTime = hour,
            Consumption = list.Count == 0 ? null : list.Sum(r => r.Consumption) / list.Count,
            Production = list.Count == 0 ? null : list.Sum(r => r.Production) / list.Count
        };
    }

    private static IEnumerable<RealTimeReading> ReadFrom(string file)
    {
        if (!File.Exists(file))
            yield break;

        using var reader = File.OpenText(file);

        for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
            if (line.StartsWith("DateTime")) 
                continue;

            var fields = line.Split(';');

            var record = new RealTimeReading
            {
                DateTime = DateTime.Parse(fields[0], CultureInfo.InvariantCulture),
                Consumption = int.Parse(fields[1], CultureInfo.InvariantCulture),
                Production = int.Parse(fields[2], CultureInfo.InvariantCulture)
            };

            yield return record;
        }
    }
}