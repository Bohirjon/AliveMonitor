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

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddScoped<HealthCheckExecutor>();
builder.Services.AddScoped<HealthCheckScheduler>();
builder.Services.AddScoped<AlertDispatcher>();
builder.Services.AddScoped<IAlertService, EmailAlertService>();
builder.Services.AddScoped<IEndpointStatusNotifier, AliveMonitor.Api.Hubs.EndpointStatusNotifier>();

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<AliveMonitor.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AliveMonitor.Api.Hubs.EndpointStatusHub>("/hubs/endpoint-status");
app.UseHangfireDashboard("/hangfire");

// Auto-migrate database + sync schedules on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var scheduler = scope.ServiceProvider.GetRequiredService<HealthCheckScheduler>();
    await scheduler.SyncAllSchedulesAsync();
}

app.Run();
