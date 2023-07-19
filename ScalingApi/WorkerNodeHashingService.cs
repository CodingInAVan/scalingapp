using System.Security.Cryptography;

namespace ScalingApi
{
	public class WorkerNodeHashingService : IWorkerNodeHashingService
    {
		private ILogger _logger;
		private readonly SortedDictionary<long, WorkerNode> _sortedHashDic = new();

		public WorkerNodeHashingService(ILogger<WorkerNodeHashingService> logger)
		{
			_logger = logger;
		}

		public void Add(WorkerNode worker)
		{
			var hash = ComputeHash(worker.Name);
            _sortedHashDic[hash] = worker;
		}

		public WorkerNode GetPreviousWorker(WorkerNode node)
		{
			long hash = ComputeHash(node.Name);

			var prev = _sortedHashDic.Where(kv => kv.Key < hash);

			if (prev.Any())
			{
				return _sortedHashDic[prev.First().Key];
			}
			if (_sortedHashDic.ContainsKey(hash))
			{
				return _sortedHashDic[hash];
			}
            return node;
        }

		public WorkerNode GetNextWorker(WorkerNode node)
		{
			long hash = ComputeHash(node.Name);

			var next = _sortedHashDic.Where(kv => kv.Key > hash);

			if (next.Any())
			{
				return _sortedHashDic[next.First().Key];
			}
			if (_sortedHashDic.ContainsKey(hash))
			{
				return _sortedHashDic[hash];
			}
            return node;
        }

		public WorkerNode? Get(string name)
		{
			if (_sortedHashDic.Count == 0)
			{
				_logger.LogError("No workers available");
				return null;
			} else
			{
                long hash = ComputeHash(name);
                if (!_sortedHashDic.ContainsKey(hash))
                {
                    var next = _sortedHashDic.Where(kv => kv.Key > hash);
                    hash = next.Any() ? next.First().Key : _sortedHashDic.First().Key;
                }
                return _sortedHashDic[hash];
            }
		}

		private static long ComputeHash(string name)
        {
            byte[] bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name));

            return BitConverter.ToInt64(bytes, 0);
        }
    }
}

