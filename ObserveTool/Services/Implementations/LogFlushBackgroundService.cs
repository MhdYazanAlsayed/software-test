using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ObserveTool.Contexts;
using ObserveTool.Options;
using ObserveTool.Services.Interfaces;

namespace ObserveTool.Services.Implementations
{
    /// <summary>
    /// Background service responsible for flushing buffered log entries to the database.
    ///
    /// Log entries are temporarily stored in an in-memory buffer to avoid blocking
    /// incoming requests. This service periodically dequeues log batches and
    /// persists them to the database using batch inserts.
    ///
    /// The flush interval and batch size are controlled via MonitoringOptions.
    /// </summary>
    public sealed class LogFlushBackgroundService : BackgroundService
    {
        //private readonly IServiceScopeFactory _scopeFactory;
        private readonly MonitoringDbContext _context;
        private readonly ILogEntryBuffer _buffer;
        private readonly ILogRequestBuffer _requestBuffer;
        private readonly MonitoringOptions _opt;

        public LogFlushBackgroundService(IServiceScopeFactory scopeFactory,
            ILogEntryBuffer buffer, 
            ILogRequestBuffer requestBuffer,
            IOptions<MonitoringOptions> opt)
        {
            var scope = scopeFactory.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();
            _buffer = buffer;
            _requestBuffer = requestBuffer;
            _opt = opt.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_opt.FlushIntervalSeconds), stoppingToken);

                    var batch = _buffer.DequeueBatch(_opt.BatchSize);
                    if (batch.Count != 0)
                    {
                        _context.Logs.AddRange(batch);
                    }

                    var requestBatch = _requestBuffer.DequeueBatch(_opt.BatchSize);
                    if (requestBatch.Count != 0)
                    {
                        _context.RequestLogs.AddRange(requestBatch);
                    }

                    await _context.SaveChangesAsync(stoppingToken);
                }
                catch
                {
                }
            }
        }
    }
}
