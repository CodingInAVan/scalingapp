using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Npgsql;

namespace WorkerNode;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly string _podName;
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;
    private const string baseApi = "http://scalingapi-service/worker/";

    public Worker(ILogger<Worker> logger, IOptions<DatabaseSettings> databaseSettings, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        try
        {
            _podName = Environment.GetEnvironmentVariable("HOSTNAME");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when retrieving the HOSTNAME environment variable.");
        }
        _connectionString = databaseSettings.Value.ConnectionString;
        _logger.LogInformation("Connection String: " + _connectionString);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker[{podName}] running at: {time}", _podName, DateTimeOffset.Now);

            var request = new HttpRequestMessage(HttpMethod.Put, baseApi + _podName);
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to ping the api server.");
            }

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var command = new NpgsqlCommand("SELECT ID FROM Job WHERE CurrentWorker = @CurrentWorker", connection);
            command.Parameters.AddWithValue("@CurrentWorker", _podName);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                _logger.LogInformation("Worker[{WorkerName}] - Job ID {JobId}", _podName, reader.GetGuid(0));
            }
            reader.Close();

            command = new NpgsqlCommand("Update Job SET Value = Value + 1 WHERE CurrentWorker = @CurrentWorker", connection);
            command.Parameters.AddWithValue("@CurrentWorker", _podName);
            command.ExecuteNonQuery();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

