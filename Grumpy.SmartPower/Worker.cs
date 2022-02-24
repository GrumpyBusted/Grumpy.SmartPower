using Grumpy.SmartPower.Core.Interface;

namespace Grumpy.SmartPower;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISmartPowerService _smartPowerService;

    public Worker(ILogger<Worker> logger, ISmartPowerService smartPowerService)
    {
        _logger = logger;
        _smartPowerService = smartPowerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastCalibrate = DateTime.Now.AddDays(-1);
        var lastModelUpdate = DateTime.Now.AddDays(-1);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            _logger.LogInformation("Worker running at: {time}", now);

            _smartPowerService.SaveData(now);

            if (now.ToString("yyyy-MM-ddTHH") != lastModelUpdate.ToString("yyyy-MM-ddTHH"))
            {
                lastModelUpdate = now;
                _smartPowerService.UpdateModel(now);
            }

            if (now.ToString("yyyy-MM-ddTHH") != lastCalibrate.ToString("yyyy-MM-ddTHH"))
            {
                lastCalibrate = now;
                _smartPowerService.Execute(now);
            }

            await Task.Delay(60000, stoppingToken);
        }
    }
}