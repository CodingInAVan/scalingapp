namespace ScalingApi
{
	public interface IJobRepository
	{
        List<Job> GetAll();
		List<Job> GetZombieJobs(List<string> activeWorkerNames);
		List<Job> GetAllByWorkerName(string name);
		void Update(Job job);
	}
}

