using System.Diagnostics;
using System.Text.Json;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Enums;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AliveMonitor.Infrastructure.Services;

public class HealthCheckExecutor(IServiceScopeFactory scopeFactory, ILogger<HealthCheckExecutor> logger)
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(120) };

    public async Task ExecuteAsync(Guid endpointId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var endpoint = await db.MonitoredEndpoints.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == endpointId);

        if (endpoint is null || !endpoint.IsEnabled)
        {
            logger.LogWarning("Endpoint {EndpointId} not found or disabled, skipping check", endpointId);
            return;
        }

        var log = await PerformCheck(endpoint);

        db.HealthCheckLogs.Add(log);

        var previousStatus = endpoint.CurrentStatus;
        endpoint.CurrentStatus = log.IsHealthy ? EndpointStatus.Healthy : EndpointStatus.Unhealthy;
        endpoint.LastCheckedAt = log.CheckedAt;
        endpoint.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Dispatch alerts
        var alertDispatcher = scope.ServiceProvider.GetService<AlertDispatcher>();
        if (alertDispatcher is not null)
        {
            await alertDispatcher.ProcessCheckResultAsync(endpoint, log);
        }

        // Notify real-time if status changed
        if (previousStatus != endpoint.CurrentStatus)
        {
            var notifier = scope.ServiceProvider.GetService<IEndpointStatusNotifier>();
            if (notifier is not null)
            {
                await notifier.NotifyStatusChangedAsync(endpoint.UserId, endpoint.Id, endpoint.CurrentStatus, endpoint.LastCheckedAt);
            }
        }

        logger.LogInformation("Health check for {Name} ({Url}): {Status} in {ResponseTime}ms",
            endpoint.FriendlyName, endpoint.Url, log.IsHealthy ? "Healthy" : "Unhealthy", log.ResponseTimeMs);
    }

    private static async Task<HealthCheckLog> PerformCheck(MonitoredEndpoint endpoint)
    {
        var stopwatch = Stopwatch.StartNew();
        var checkLog = new HealthCheckLog
        {
            EndpointId = endpoint.Id,
            CheckedAt = DateTime.UtcNow,
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint.Url);

            if (endpoint.CustomHeadersJson is not null)
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.CustomHeadersJson);
                if (headers is not null)
                {
                    foreach (var (key, value) in headers)
                    {
                        request.Headers.TryAddWithoutValidation(key, value);
                    }
                }
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(endpoint.TimeoutSeconds));
            var response = await HttpClient.SendAsync(request, cts.Token);

            stopwatch.Stop();
            checkLog.HttpStatusCode = (int)response.StatusCode;
            checkLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            var statusCodeMatch = (int)response.StatusCode == endpoint.ExpectedStatusCode;
            var jsonMatch = true;

            if (!string.IsNullOrWhiteSpace(endpoint.JsonPropertyName))
            {
                try
                {
                    var body = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty(endpoint.JsonPropertyName, out var prop))
                    {
                        jsonMatch = prop.ToString() == endpoint.JsonPropertyExpectedValue;
                    }
                    else
                    {
                        jsonMatch = false;
                    }
                }
                catch
                {
                    jsonMatch = false;
                }
            }

            checkLog.IsHealthy = statusCodeMatch && jsonMatch;
            if (!checkLog.IsHealthy)
            {
                checkLog.ErrorMessage = $"Status code: {(int)response.StatusCode} (expected {endpoint.ExpectedStatusCode})"
                    + (jsonMatch ? "" : $", JSON property mismatch");
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            checkLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            checkLog.IsHealthy = false;
            checkLog.ErrorMessage = $"Request timed out after {endpoint.TimeoutSeconds}s";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            checkLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            checkLog.IsHealthy = false;
            checkLog.ErrorMessage = ex.Message.Length > 2048 ? ex.Message[..2048] : ex.Message;
        }

        return checkLog;
    }
}
