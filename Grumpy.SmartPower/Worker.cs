using Grumpy.SmartPower.Core.Interface;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower;

public class Worker : BackgroundService
{
    private readonly WorkerOptions _options;
    private readonly ILogger<Worker> _logger;
    private readonly ISmartPowerService _smartPowerService;

    public Worker(IOptions<WorkerOptions> options, ILogger<Worker> logger, ISmartPowerService smartPowerService)
    {
        _options = options.Value;
        _logger = logger;
        _smartPowerService = smartPowerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastCalibrate = DateTime.Now.AddDays(-1);
        var lastModelUpdate = DateTime.Now;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            if (lastCalibrate - DateTime.Now > TimeSpan.FromMilliseconds(_options.Interval))
            {
                lastCalibrate = DateTime.Now;
                _smartPowerService.Execute(DateTime.Now);
            }

            if (DateTime.Now.Hour != lastModelUpdate.Hour)
            {
                lastModelUpdate = DateTime.Now;
                //_smartPowerService.UpdateModel();
            }

            _smartPowerService.SaveData();

            await Task.Delay(60000, stoppingToken);
        }
    }
}