var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(false);

var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var googleClientId = builder.AddParameter("google-client-id", secret: true);
var smtpHost = builder.AddParameter("smtp-host", secret: false);
var smtpPort = builder.AddParameter("smtp-port", secret: false);
var smtpUser = builder.AddParameter("smtp-user", secret: true);
var smtpPassword = builder.AddParameter("smtp-password", secret: true);
var senderAddress = builder.AddParameter("alert-sender-address", secret: false);
var telegramBotToken = builder.AddParameter("telegram-bot-token", secret: true);
var hangfireUser = builder.AddParameter("hangfire-user", secret: true);
var hangfirePassword = builder.AddParameter("hangfire-password", secret: true);
var allowedOrigin = builder.AddParameter("allowed-origin", secret: false);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("alivemonitordb");

var frontend = builder.AddViteApp("frontend", "../../../frontend")
    .WithNpm()
    .WithContainerFilesSource("/app/dist")
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

builder.AddProject<Projects.AliveMonitor_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("GoogleAuth__ClientId", googleClientId)
    .WithEnvironment("Alerts__Email__SmtpHost", smtpHost)
    .WithEnvironment("Alerts__Email__SmtpPort", smtpPort)
    .WithEnvironment("Alerts__Email__Username", smtpUser)
    .WithEnvironment("Alerts__Email__Password", smtpPassword)
    .WithEnvironment("Alerts__Email__SenderAddress", senderAddress)
    .WithEnvironment("Alerts__Telegram__BotToken", telegramBotToken)
    .WithEnvironment("Hangfire__DashboardUser", hangfireUser)
    .WithEnvironment("Hangfire__DashboardPassword", hangfirePassword)
    .WithEnvironment("AllowedOrigins__0", allowedOrigin)
    .PublishWithContainerFiles(frontend, "/app/wwwroot");

builder.Build().Run();
