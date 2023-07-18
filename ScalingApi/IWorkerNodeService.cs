namespace ScalingApi
{
	public interface IWorkerNodeService
	{
        void UpdateWorkerNode(WorkerNode node);
        List<string> GetActiveWorkerNodeNames();
    }
}

