namespace ScalingApi
{
	public class WorkerNodeManager : IWorkerNodeManager
    {
		private readonly WorkerNodeHashingService _workerNodeHashingService;
		private readonly List<WorkerNode> _workers = new();
		private readonly IJobRepository _jobRepository;
		private readonly IWorkerNodeService _workerNodeService;
		private readonly ILogger<WorkerNodeManager> _logger;

        public WorkerNodeManager(IJobRepository jobRepository, IWorkerNodeService workerNodeService, ILogger<WorkerNodeManager> logger)
		{
            _workerNodeHashingService = new WorkerNodeHashingService();
            _jobRepository = jobRepository;
			_workerNodeService = workerNodeService;
			_logger = logger;
        }

		private void RemoveWorkerNode(WorkerNode worker)
		{
			var jobs = _jobRepository.GetAllByWorkerName(worker.Name);
            foreach (var job in jobs)
            {
                var unassignedJob = job with { CurrentWorker = "" };
                _jobRepository.Update(unassignedJob);
                _logger.LogInformation("Assigned worker has been removed for Job[{JobId}]", job.Id);
            }
			_workers.Remove(worker);
            _logger.LogInformation("Worker[{WorkerName}] has been removed", worker.Name);
        }

		private void RegisterWorkerNode(WorkerNode worker)
		{
			var prevWorker = _workerNodeHashingService.GetPreviousWorker(worker);
			var nextWorker = _workerNodeHashingService.GetNextWorker(worker);

			var allJobs = new List<Job>();
			var prevWorkersJobs = _jobRepository.GetAllByWorkerName(prevWorker.Name);
			allJobs.AddRange(prevWorkersJobs);

			if(!prevWorker.Name.Equals(nextWorker.Name))
			{
				allJobs.AddRange(_jobRepository.GetAllByWorkerName(nextWorker.Name));
            }

            _workers.Add(worker);
            _workerNodeHashingService.Add(worker);
            AssignJobs(allJobs);
		}

		private void AssignUnassignedJobs()
		{
			AssignJobs(_jobRepository.GetAllByWorkerName(string.Empty));
		}

		private void AssignJobs(List<Job> jobs)
		{
            foreach (var job in jobs)
            {
                var worker = _workerNodeHashingService.Get(job.Id.ToString());
				_jobRepository.Update(new Job { Id = job.Id, CurrentWorker = worker.Name });
            }
        }

		private void RemoveWorkerFromJobs(List<Job> jobs)
		{
			foreach (var job in jobs)
			{
				_jobRepository.Update(new Job { Id = job.Id, CurrentWorker = string.Empty });
			}
		}

		public void SyncWorkers()
		{
			var registeredWorkerSet = _workers.ToHashSet();
            var currentRunningWorkerNamesSet = _workerNodeService.GetActiveWorkerNodeNames().ToHashSet();
            List<WorkerNode> disconnectedWorkerNodes = new();

			foreach(var workerNode in _workers)
			{
				if(!currentRunningWorkerNamesSet.Contains(workerNode.Name))
				{
                    disconnectedWorkerNodes.Add(workerNode);
				} else
				{
                    currentRunningWorkerNamesSet.Remove(workerNode.Name);
				}
			}
			// Remove all disconnectedWorkerNodes
			foreach(var workerNode in disconnectedWorkerNodes)
			{
				RemoveWorkerNode(workerNode);
			}

            // register the unregistered workers
            foreach (var workerName in currentRunningWorkerNamesSet)
            {
				var workerNode = new WorkerNode() { Name = workerName, UpdatedAt = DateTimeOffset.UtcNow };
				RegisterWorkerNode(workerNode);
			}

			// Get all zombie jobs whose assigned job does not exist anymore and unassign it.
			var zombieJobs = _jobRepository.GetZombieJobs(_workers.Select(w => w.Name).ToList());
			RemoveWorkerFromJobs(zombieJobs);


            AssignUnassignedJobs();
        }
    }
}

