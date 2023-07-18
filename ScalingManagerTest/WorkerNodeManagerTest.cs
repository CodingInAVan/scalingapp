using Microsoft.Extensions.Logging.Abstractions;
using ScalingApi;

namespace ScalingManagerTest
{
    [TestClass]
    public class WorkerNodeManagerTest
    {
        public class MockJobRepository : IJobRepository
        {
            private readonly List<Job> _jobs;

            public MockJobRepository(List<Job> jobs)
            {
                _jobs = jobs;
            }

            public void Delete(Guid id)
            {
                var index = -1;
                for(int i=0; i<_jobs.Count; i++)
                {
                    if (_jobs[i].Id.Equals(id))
                    {
                        index = i;
                    }
                }
                if (index > -1)
                {
                    _jobs.RemoveAt(index);
                }
            }

            public List<Job> GetAllByWorkerName(string name)
            {
                return _jobs.Where(job => job.CurrentWorker.Equals(name)).ToList();
            }

            public void Update(Job updatingJob)
            {
                int index = _jobs.FindIndex(j => j.Id == updatingJob.Id);
                if (index != -1)
                {
                    _jobs[index] = new Job { Id = updatingJob.Id, CurrentWorker = updatingJob.CurrentWorker };
                }
            }

            public List<Job> GetAll()
            {
                return _jobs;
            }

            public List<Job> GetZombieJobs(List<string> activeWorkerNames)
            {
                var activeWorkerNamesSet = activeWorkerNames.ToHashSet();
                return _jobs.Where(job => !activeWorkerNamesSet.Contains(job.CurrentWorker)).ToList();
            }
        }
        private WorkerNodeManager _manager;
        private IJobRepository _jobRepository;
        private IWorkerNodeService _workerNodeService = new WorkerNodeService(2);

        private List<Job> _jobData = new();

        [TestInitialize]
        public void SetUp()
        {
            _jobRepository = new MockJobRepository(_jobData);
            _manager = new WorkerNodeManager(_jobRepository, _workerNodeService, new NullLogger<WorkerNodeManager>());
        }

        // Given jobs which have a pre-generated Guid should be assigend worker correctly.
        // After adding a new worker, jobs should be redistributed based on the closest nodes (previous and next).
        [TestMethod]
        public void SyncTest()
        {
            _workerNodeService.AddWorkerNode(new WorkerNode() { Name = "pod-1 99999", UpdatedAt = DateTimeOffset.UtcNow });
            _workerNodeService.AddWorkerNode(new WorkerNode() { Name = "pod-2 12345", UpdatedAt = DateTimeOffset.UtcNow });

            var testJobs = new List<Job>()
            {
                new Job() { Id = Guid.Parse("61c21066-7f10-4885-b2cd-f58b5405dee7"), CurrentWorker = "" },
                new Job() { Id = Guid.Parse("54f30d85-474d-4652-8d1c-6039c02d0e78"), CurrentWorker = "" },
                new Job() { Id = Guid.Parse("b8722ee6-008c-49d3-befc-4742d23055c7"), CurrentWorker = "" },
                new Job() { Id = Guid.Parse("429d28ee-7c46-4ff5-bf51-c0e68384495a"), CurrentWorker = "" },
                new Job() { Id = Guid.Parse("65679312-0af4-4f79-aef2-c3bfb24d7d0a"), CurrentWorker = "" }
            };

            foreach(var job in testJobs)
            {
                _jobData.Add(job);
            }

            _manager.SyncWorkers();

            var jobs = _jobRepository.GetAll();

            Assert.AreEqual(jobs.Where(job => job.CurrentWorker == "pod-1 99999").ToList().Count, 2);
            Assert.AreEqual(jobs.Where(job => job.CurrentWorker == "pod-2 12345").ToList().Count, 3);

            _workerNodeService.AddWorkerNode(new WorkerNode() { Name = "pod-3 00000", UpdatedAt = DateTimeOffset.UtcNow });

            _manager.SyncWorkers();

            Assert.AreEqual(jobs.Where(job => job.CurrentWorker == "pod-1 99999").ToList().Count, 2);
            Assert.AreEqual(jobs.Where(job => job.CurrentWorker == "pod-2 12345").ToList().Count, 2);
            Assert.AreEqual(jobs.Where(job => job.CurrentWorker == "pod-3 00000").ToList().Count, 1);

            _jobData.Clear();
        }
    }
}