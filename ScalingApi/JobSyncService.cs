using System;

namespace ScalingApi
{
	public class JobSyncService : BackgroundService
	{
        private readonly ILogger _logger;
        private readonly IWorkerNodeManager _workerNodeManager;

		public JobSyncService(ILogger<JobSyncService> logger, IWorkerNodeManager workerNodeManager)
		{
            _logger = logger;
            _workerNodeManager = workerNodeManager;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Sync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private Task Sync(CancellationToken stoppingToken)
        {
            _workerNodeManager.SyncWorkers();
            _logger.LogInformation("Sync is done at {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}

