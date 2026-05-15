using System.Text;
using System.Text.Json.Serialization;
using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using AliveMonitor.Infrastructure.Repositories;
using AliveMonitor.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Aspire-managed PostgreSQL + EF Core
builder.AddNpgsqlDbContext<AppDbContext>("alivemonitordb");

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection(GoogleAuthSettings.SectionName));
builder.Services.Configure<AlertSettings>(builder.Configuration.GetSection(AlertSettings.SectionName));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IEndpointRepository, EndpointRepository>();
builder.Services.AddScoped<IHealthCheckLogRepository, HealthCheckLogRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITelegramLinkCodeRepository, TelegramLinkCodeRepository>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddScoped<HealthCheckExecutor>();
builder.Services.AddScoped<HealthCheckScheduler>();
builder.Services.AddScoped<AlertRecipientResolver>();
builder.Services.AddScoped<AlertDispatcher>();
builder.Services.AddScoped<EmailAlertService>();
builder.Services.AddScoped<TelegramAlertService>();
builder.Services.AddScoped<WebhookAlertService>();
builder.Services.AddScoped<IAlertService, CompositeAlertService>();
builder.Services.AddScoped<TelegramLinkCodeService>();
builder.Services.AddScoped<SslCertificateChecker>();
builder.Services.AddScoped<SslCheckScheduler>();
builder.Services.AddScoped<IEndpointStatusNotifier, AliveMonitor.Api.Hubs.EndpointStatusNotifier>();
builder.Services.AddHostedService<TelegramBotService>();

// SignalR
builder.Services.AddSignalR();

// Hangfire
var connectionString = builder.Configuration.GetConnectionString("alivemonitordb");
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(opts =>
        opts.UseNpgsqlConnection(connectionString ?? "")));
builder.Services.AddHangfireServer();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero,
        };

        // Allow JWT via query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "Enter your JWT token",
            },
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });
        }

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AliveMonitor API v1");
    });
}

app.UseMiddleware<AliveMonitor.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<AliveMonitor.Api.Hubs.EndpointStatusHub>("/hubs/endpoint-status");

var hangfireUser = builder.Configuration["Hangfire:DashboardUser"];
var hangfirePassword = builder.Configuration["Hangfire:DashboardPassword"];
if (!string.IsNullOrWhiteSpace(hangfireUser) && !string.IsNullOrWhiteSpace(hangfirePassword))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[]
        {
            new AliveMonitor.Api.Middleware.HangfireBasicAuthFilter(hangfireUser, hangfirePassword)
        }
    });
}

app.MapFallbackToFile("index.html");

// Auto-migrate database + sync schedules on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var scheduler = scope.ServiceProvider.GetRequiredService<HealthCheckScheduler>();
    await scheduler.SyncAllSchedulesAsync();

    var sslScheduler = scope.ServiceProvider.GetRequiredService<SslCheckScheduler>();
    sslScheduler.Schedule();
}

app.Run();
