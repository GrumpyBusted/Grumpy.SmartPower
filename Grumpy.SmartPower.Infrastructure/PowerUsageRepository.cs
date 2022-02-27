using Grumpy.Common.Extensions;
using Grumpy.SmartPower.Core.Infrastructure;
using Microsoft.Extensions.Options;
using Grumpy.Caching.Interface;

namespace Grumpy.SmartPower.Infrastructure;

public class PowerUsageRepository : IPowerUsageRepository
{
    private readonly PowerUsageRepositoryOptions _options;

    public PowerUsageRepository(IOptions<PowerUsageRepositoryOptions> options)
    {
        _options = options.Value;
    }

    public void Save(DateTime hour, int consumption, int production, int gridFeedIn, int gridFeedOut, int meterReading, int batteryLevel, int batteryCurrent, double price)
    {
        var record = new PowerUsage
        {
            Hour = hour,
            Consumption = consumption,
            Production = production,
            GridFeedIn = gridFeedIn,
            GridFeedOut = gridFeedOut,
            MeterReading = meterReading,
            BatteryLevel = batteryLevel,
            BatteryCurrent = batteryCurrent,
            Price = price
        };

        var folder = Path.GetDirectoryName(_options.RepositoryPath) ?? ".";

        if (folder != "" && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        if (!File.Exists(_options.RepositoryPath))
            File.WriteAllText(_options.RepositoryPath, record.CsvHeader(';') + Environment.NewLine);

        File.AppendAllText(_options.RepositoryPath, record.CsvRecord(';') + Environment.NewLine);
    }
}