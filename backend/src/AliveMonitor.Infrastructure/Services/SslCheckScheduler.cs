using Hangfire;
using Microsoft.Extensions.Logging;

namespace AliveMonitor.Infrastructure.Services;

public class SslCheckScheduler(IRecurringJobManager recurringJobManager, ILogger<SslCheckScheduler> logger)
{
    public void Schedule()
    {
        recurringJobManager.AddOrUpdate<SslCertificateChecker>(
            "ssl-certificate-check",
            checker => checker.CheckAllAsync(),
            "0 6 * * *"); // Daily at 6 AM UTC

        logger.LogInformation("Scheduled SSL certificate check job (daily at 06:00 UTC)");
    }
}
