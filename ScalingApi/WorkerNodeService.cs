namespace ScalingApi
{
    public class WorkerNodeService : IWorkerNodeService
    {
        private readonly List<WorkerNode> _workers = new();
        private readonly object _lock = new object();
        private readonly int _workerExpiryTimeInMinutes;

        public WorkerNodeService(int workerExpiryTimeInMinutes)
        {
            _workerExpiryTimeInMinutes = workerExpiryTimeInMinutes;
        }

        public List<string> GetActiveWorkerNodeNames()
        {
            lock (_lock)
            {
                var currentTime = DateTimeOffset.UtcNow;
                var activeWorkerNodes = _workers.Where(worker => currentTime - worker.UpdatedAt < TimeSpan.FromMinutes(_workerExpiryTimeInMinutes)).ToList();

                var workersToDelete = _workers.Except(activeWorkerNodes).ToList();

                foreach (var worker in workersToDelete)
                {
                    _workers.Remove(worker);
                }

                return activeWorkerNodes.Select(worker => worker.Name).ToList();
            }
        }

        public void UpdateWorkerNode(WorkerNode node)
        {
            lock (_lock)
            {
                var updatingNode = _workers.FirstOrDefault(worker => worker.Name == node.Name);

                if (updatingNode != null)
                {
                    updatingNode.UpdatedAt = node.UpdatedAt;
                } else
                {
                    _workers.Add(node);
                }
            }
        }
    }
}

