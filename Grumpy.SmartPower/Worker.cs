using Grumpy.SmartPower.Core.Interface;
using Microsoft.Extensions.Options;

namespace Grumpy.SmartPower
{
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
                var now = DateTime.Now;

                _logger.LogInformation("Worker running at: {time}", now);

                if (now - lastCalibrate > TimeSpan.FromMilliseconds(_options.Interval))
                {
                    lastCalibrate = now;
                    _smartPowerService.Execute(now);
                }

                if (now.Hour != lastModelUpdate.Hour)
                {
                    lastModelUpdate = now;
                    //_smartPowerService.UpdateModel();
                }

                _smartPowerService.SaveData(now);

                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}