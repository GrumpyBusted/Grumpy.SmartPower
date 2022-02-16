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
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _smartPowerService.Execute(DateTime.Now);

                await Task.Delay(_options.Interval, stoppingToken);
            }
        }
    }
}