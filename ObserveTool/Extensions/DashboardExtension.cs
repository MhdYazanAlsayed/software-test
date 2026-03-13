using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ObserveTool.Contexts;
using ObserveTool.Helpers;
using ObserveTool.Middlewares;
using ObserveTool.Models;
using ObserveTool.Models.Enums;

namespace ObserveTool.Extensions
{
    public static class DashboardExtension
    {
        /// <summary>
        /// Registers the monitoring dashboard and related APIs.
        /// 
        /// This method exposes a secured monitoring UI and a set of APIs used
        /// to retrieve application logs and request statistics. The dashboard
        /// endpoint is protected using BasicAuthMiddleware and serves a static
        /// embedded HTML page that consumes the monitoring APIs.
        /// </summary>
        public static WebApplication UseMonitorDashboard (this WebApplication app)
        {
            app.UseMiddleware<BasicAuthMiddleware>();

            app.MapGet("/monitoring-83251", async context =>
            {
                var html = EmbeddedFileReader.Read("index.html");

                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
            });

            app.UseMonitorAPIs();

            return app;
        }

        /// <summary>
        /// Maps monitoring APIs used by the dashboard to query application logs.
        /// 
        /// The endpoints provide:
        /// - Log statistics (errors, warnings, informational logs)
        /// - Paginated access to application logs
        /// - Paginated access to HTTP request logs with filtering capabilities
        /// 
        /// These APIs are intended for internal monitoring and diagnostics.
        /// Data is retrieved from MonitoringDbContext and optimized using
        /// filtering, pagination, and projection to avoid loading large payloads.
        /// </summary>
        internal static WebApplication UseMonitorAPIs(this WebApplication app)
        {
            // Statistics
            app.MapGet("/monitoring-83251/api/stats", async (MonitoringDbContext db) =>
            {
                var fromDate = DateTimeOffset.UtcNow.AddDays(-1);

                var logs = db.Logs.Where(x => x.Timestamp >= fromDate);

                var result = new
                {
                    Errors = await logs.CountAsync(x => x.Level == LogLevel.Error || x.Level == LogLevel.Critical),
                    Informations = await logs.CountAsync(x => x.Level == LogLevel.Information),
                    Warnings = await logs.CountAsync(x => x.Level == LogLevel.Warning),
                    Total = await logs.CountAsync()
                };

                return result;
            });

            // Data
            app.MapGet("/monitoring-83251/api/logs",
            async (
                MonitoringDbContext db,
                LogLevel? level,
                int page = 1,
                int pageSize = 20) =>
            {
                var query = db.Logs.AsQueryable();

                if (level.HasValue)
                    query = query.Where(x => x.Level == level.Value);

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.Timestamp,
                        x.LevelText,
                        x.Category,
                        x.Message,
                        HasException = x.Exception != null
                    })
                    .ToListAsync();

                return new
                {
                    total,
                    page,
                    pageSize,
                    data
                };
            });

            app.MapGet("/monitoring-83251/api/requestlogs", async (
                MonitoringDbContext db,
                string? method,              
                string? path,                
                string? ip,                 
                DateTime? from,             
                DateTime? to,                  
                int page = 1,
                int pageSize = 20) =>
            {
                var query = db.RequestLogs.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(method) && Enum.TryParse<RequestMethod>(method, true, out var methodEnum))
                    query = query.Where(x => x.Method == methodEnum);

                if (!string.IsNullOrEmpty(path))
                    query = query.Where(x => x.Path != null && x.Path.Contains(path));

                if (!string.IsNullOrEmpty(ip))
                    query = query.Where(x => x.IpAddress != null && x.IpAddress.Contains(ip));

                if (from.HasValue)
                    query = query.Where(x => x.Timestamp >= from.Value);  // Assuming you have a Timestamp property (you may need to add it)

                if (to.HasValue)
                    query = query.Where(x => x.Timestamp <= to.Value);

                // Get total count for pagination
                var total = await query.CountAsync();

                // Fetch paginated data (excluding large fields)
                var data = await query
                    .OrderByDescending(x => x.Id)  // or use a Timestamp property if available
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.ElapsedMilliseconds,
                        x.Path,
                        Method = x.Method.ToString(),
                        x.IpAddress,
                        x.TraceId,
                        // Optionally include a flag if request/response exist (but not the content)
                        HasRequest = x.Request != null,
                        HasResponse = x.Response != null
                    })
                    .ToListAsync();

                return Results.Ok(new
                {
                    total,
                    page,
                    pageSize,
                    data
                });
            });
            
            return app;
        }
    
    }
}
