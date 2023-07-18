using WorkerNode;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient<Worker>();
        services.Configure<DatabaseSettings>(hostContext.Configuration.GetSection(nameof(DatabaseSettings)));
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

