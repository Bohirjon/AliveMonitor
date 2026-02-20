using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AliveMonitor.Infrastructure.Services;

public class SslCertificateChecker(IServiceScopeFactory scopeFactory, ILogger<SslCertificateChecker> logger)
{
    private static readonly int[] Thresholds = [1, 7, 30];

    public async Task CheckAllAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var endpoints = await db.MonitoredEndpoints
            .Include(e => e.User)
            .Where(e => e.IsEnabled && e.SslCheckEnabled)
            .ToListAsync();

        logger.LogInformation("Starting SSL certificate check for {Count} endpoints", endpoints.Count);

        foreach (var endpoint in endpoints)
        {
            try
            {
                var log = await FetchCertificateInfo(endpoint);

                db.SslCertificateCheckLogs.Add(log);
                endpoint.SslLastCheckedAt = log.CheckedAt;
                endpoint.SslCertificateExpiresAt = log.ExpiresAt;
                endpoint.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                if (log.IsValid && log.DaysUntilExpiry is not null)
                {
                    await ProcessSslAlertAsync(scope.ServiceProvider, endpoint, log);
                }

                logger.LogInformation(
                    "SSL check for {Name}: valid={IsValid}, expires={ExpiresAt}, days={Days}",
                    endpoint.FriendlyName, log.IsValid, log.ExpiresAt, log.DaysUntilExpiry);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed SSL check for {Name} ({Url})", endpoint.FriendlyName, endpoint.Url);
            }
        }
    }

    private static async Task<SslCertificateCheckLog> FetchCertificateInfo(MonitoredEndpoint endpoint)
    {
        var log = new SslCertificateCheckLog
        {
            EndpointId = endpoint.Id,
            CheckedAt = DateTime.UtcNow,
        };

        X509Certificate2? capturedCert = null;

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, cert, _, sslPolicyErrors) =>
            {
                if (cert is not null)
                    capturedCert = new X509Certificate2(cert);
                return true; // Accept all to capture the cert even if invalid
            },
        };

        try
        {
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            using var request = new HttpRequestMessage(HttpMethod.Head, endpoint.Url);
            await client.SendAsync(request);

            if (capturedCert is not null)
            {
                log.IsValid = true;
                log.SubjectName = capturedCert.Subject;
                log.IssuerName = capturedCert.Issuer;
                log.ExpiresAt = capturedCert.NotAfter.ToUniversalTime();
                log.DaysUntilExpiry = (int)(capturedCert.NotAfter.ToUniversalTime() - DateTime.UtcNow).TotalDays;
            }
            else
            {
                log.IsValid = false;
                log.ErrorMessage = "No SSL certificate returned by server";
            }
        }
        catch (Exception ex)
        {
            log.IsValid = false;
            log.ErrorMessage = ex.Message.Length > 2048 ? ex.Message[..2048] : ex.Message;
        }
        finally
        {
            capturedCert?.Dispose();
        }

        return log;
    }

    private async Task ProcessSslAlertAsync(IServiceProvider services, MonitoredEndpoint endpoint, SslCertificateCheckLog log)
    {
        var days = log.DaysUntilExpiry!.Value;

        // Reset threshold tracking when cert is renewed (days > 30)
        if (days > 30)
        {
            if (endpoint.SslLastAlertedThresholdDays is not null)
            {
                endpoint.SslLastAlertedThresholdDays = null;
                logger.LogInformation("SSL threshold reset for {Name} (cert renewed, {Days} days remaining)",
                    endpoint.FriendlyName, days);
            }
            return;
        }

        // Find the crossed threshold (30, 7, or 1)
        int? crossedThreshold = null;
        foreach (var threshold in Thresholds.OrderByDescending(t => t))
        {
            if (days <= threshold)
                crossedThreshold = threshold;
        }

        if (crossedThreshold is null)
            return;

        // Skip if already alerted at this or a lower (more urgent) threshold
        if (endpoint.SslLastAlertedThresholdDays is not null && endpoint.SslLastAlertedThresholdDays <= crossedThreshold)
            return;

        var alertEmailResolver = services.GetRequiredService<AlertEmailResolver>();
        var alertService = services.GetRequiredService<IAlertService>();

        var alertEmails = await alertEmailResolver.GetAlertEmailsAsync(endpoint);
        if (alertEmails.Count > 0)
        {
            await alertService.SendSslExpirationAlertAsync(endpoint, log, crossedThreshold.Value, alertEmails);
            logger.LogInformation("SSL expiration alert sent for {Name} at {Threshold}-day threshold",
                endpoint.FriendlyName, crossedThreshold.Value);
        }

        endpoint.SslLastAlertedThresholdDays = crossedThreshold.Value;
    }
}
