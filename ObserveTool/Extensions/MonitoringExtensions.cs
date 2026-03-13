using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ObserveTool.Contexts;
using ObserveTool.Middlewares;
using ObserveTool.Options;
using ObserveTool.Services.Implementations;
using ObserveTool.Services.Implementations.BufferLogger;
using ObserveTool.Services.Implementations.Buffers;
using ObserveTool.Services.Implementations.Loggers;
using ObserveTool.Services.Interfaces;


namespace ObserveTool.Extensions
{
    public static class MonitoringExtensions
    {
        /// <summary>
        /// Registers the in-process monitoring infrastructure.
        ///
        /// This method configures all services required to capture and persist
        /// application logs and HTTP request logs.
        ///
        /// Logs are first buffered in memory to minimize request latency,
        /// then periodically flushed to the database by background services.
        ///
        /// Registered components include:
        /// - Log buffers for temporary in-memory storage
        /// - EF Core DbContext for log persistence
        /// - Custom logger provider integrated with Microsoft.Extensions.Logging
        /// - Background services responsible for flushing logs and cleaning old data
        /// </summary>
        public static IServiceCollection AddInProcessMonitoring(
            this IServiceCollection services,
            string connectionString,
            Action<MonitoringOptions>? configure = null)
        {
            services.Configure(configure ?? (_ => { }));

            services.AddSingleton<ILogEntryBuffer, LogEntryBuffer>();
            services.AddSingleton<ILogRequestBuffer, LogRequestBuffer>();

            services.AddDbContext<MonitoringDbContext>(opt =>
                opt.UseSqlServer(connectionString));

            services.AddSingleton<ILoggerProvider, BufferedLoggerProvider>();
            services.AddSingleton<IRequestLogger, RequestLogger>();

            services.AddHostedService<LogFlushBackgroundService>();
            services.AddHostedService<LogCleanupService>();

            return services;
        }


        /// <summary>
        /// Enables the in-process monitoring pipeline.
        ///
        /// On application startup this method optionally:
        /// - Applies database migrations automatically
        /// - Activates request logging middleware
        ///
        /// The behavior is controlled through MonitoringOptions.
        /// </summary>
        public static IApplicationBuilder UseInProcessMonitoring(this IApplicationBuilder app)
        {
            var opt = app.ApplicationServices.GetRequiredService<IOptions<MonitoringOptions>>().Value;

            if (opt.AutoMigrateOnStartup)
            {
                using var scope = app.ApplicationServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();
                db.Database.Migrate(); 
            }

            if (opt.EnableRequestLogging)
                app.UseMiddleware<RequestLoggingMiddleware>();

            return app;
        }
    }
}
