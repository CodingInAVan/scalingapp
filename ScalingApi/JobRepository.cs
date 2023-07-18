using System.Text;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ScalingApi
{
	public class JobRepository : IJobRepository
	{
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public JobRepository(ILogger<JobRepository> logger, IOptions<DatabaseSettings> databaseSettings)
		{
            _logger = logger;
            _connectionString = databaseSettings.Value.ConnectionString;
            _logger.LogInformation("ConnectionString = {connectionString}", _connectionString);
		}

        public List<Job> GetAll()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var command = new NpgsqlCommand("SELECT ID, CurrentWorker FROM Job", connection);

            using var reader = command.ExecuteReader();
            List<Job> jobs = new();

            while (reader.Read())
            {
                var job = new Job
                {
                    Id = reader.GetGuid(0),
                    CurrentWorker = reader.GetString(1)
                };
                jobs.Add(job);
            }
            return jobs;
        }

        public List<Job> GetAllByWorkerName(string name)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var command = new NpgsqlCommand("SELECT ID, CurrentWorker FROM Job WHERE CurrentWorker = @CurrentWorker", connection);
            command.Parameters.AddWithValue("@CurrentWorker", name);

            using var reader = command.ExecuteReader();
            List<Job> jobs = new();

            while(reader.Read())
            {
                var job = new Job
                {
                    Id = reader.GetGuid(0),
                    CurrentWorker = reader.GetString(1)
                };
                jobs.Add(job);
            }
            return jobs;
        }

        public List<Job> GetZombieJobs(List<string> activeWorkerNames)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var parameters = new List<NpgsqlParameter>();
            var inClause = new StringBuilder();
            for (int i = 0; i < activeWorkerNames.Count; i++)
            {
                var parameterName = $"@workerName{i}";
                parameters.Add(new NpgsqlParameter(parameterName, activeWorkerNames[i]));
                inClause.Append(i > 0 ? $", {parameterName}" : parameterName);
            }

            var jobs = new List<Job>();
            using var command = new NpgsqlCommand($"SELECT * FROM jobs WHERE CurrentWorker NOT IN ({inClause})", connection);
            command.Parameters.AddRange(parameters.ToArray());
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var job = new Job
                {
                    Id = reader.GetGuid(0),
                    CurrentWorker = reader.GetString(1)
                };
                jobs.Add(job);
            }
            return jobs;
        }

        public void Update(Job job)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var command = new NpgsqlCommand("UPDATE Job SET CurrentWorker = @CurrentWorker WHERE ID = @ID", connection);
            command.Parameters.AddWithValue("@ID", job.Id);
            command.Parameters.AddWithValue("@CurrentWorker", job.CurrentWorker);

            command.ExecuteNonQuery();
        }
    }
}

