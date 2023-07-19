using System;
namespace ScalingApi
{
	public interface IWorkerNodeHashingService
	{
        void Add(WorkerNode worker);

        void Remove(WorkerNode worker);

        WorkerNode GetPreviousWorker(WorkerNode node);

        WorkerNode GetNextWorker(WorkerNode node);

        WorkerNode? Get(string name);
    }
}

