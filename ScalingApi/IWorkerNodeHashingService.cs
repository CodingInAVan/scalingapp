using System;
namespace ScalingApi
{
	public interface IWorkerNodeHashingService
	{
        void Add(WorkerNode worker);

        WorkerNode GetPreviousWorker(WorkerNode node);

        WorkerNode GetNextWorker(WorkerNode node);

        WorkerNode? Get(string name);
    }
}

