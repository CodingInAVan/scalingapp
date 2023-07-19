using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScalingApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSingleton<IJobRepository, JobRepository>();
builder.Services.AddSingleton<IWorkerNodeService>(sb => new WorkerNodeService(2));
builder.Services.AddSingleton<IWorkerNodeManager, WorkerNodeManager>();
builder.Services.AddSingleton<IWorkerNodeHashingService, WorkerNodeHashingService>();
builder.Services.AddHostedService<JobSyncService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", ([FromServices] ILogger<Program> logger) =>
{
    logger.LogInformation("Server is running...");
    return "ScalingApi";
});
app.MapPut("/worker/{workerName}", (string workerName, IWorkerNodeService workerNodeService, ILogger<Program> logger) =>
{
    logger.LogInformation("Received the request " + workerName);
    workerNodeService.UpdateWorkerNode(new WorkerNode() { Name = workerName, UpdatedAt = DateTimeOffset.UtcNow });
});

app.Run();
