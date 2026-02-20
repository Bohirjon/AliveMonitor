var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("alivemonitordb");

var api = builder.AddProject<Projects.AliveMonitor_Api>("api")
    .WithReference(database)
    .WaitFor(database);

builder.AddViteApp("frontend", "../../../frontend")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("BROWSER", "none")
    .WithEndpoint("http", e =>
    {
        e.Port = 5173;
        e.IsProxied = false;
    })
    .WithEndpoint("https", e =>
    {
        e.Port = 7001;
        e.TargetPort = 5173;
    });

builder.Build().Run();