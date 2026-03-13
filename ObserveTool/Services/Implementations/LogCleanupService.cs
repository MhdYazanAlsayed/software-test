using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ObserveTool.Contexts;

namespace ObserveTool.Services.Implementations
{
    /// <summary>
    /// Background service responsible for periodically cleaning up old monitoring logs.
    ///
    /// The service runs once every 24 hours and removes log records older than
    /// the configured retention period (currently 30 days).
    ///
    /// This prevents the monitoring tables from growing indefinitely and helps
    /// maintain database performance.
    /// </summary>
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LogCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();

                var cutoff = DateTimeOffset.UtcNow.AddDays(-30);

                await db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM monitoring.Logs WHERE Timestamp < @cutoff",
                    new SqlParameter("@cutoff", cutoff));

                await db.Database.ExecuteSqlRawAsync(
                     "DELETE FROM monitoring.RequestLogs WHERE Timestamp < @cutoff",
                     new SqlParameter("@cutoff", cutoff));
            }
        }
    }
}
