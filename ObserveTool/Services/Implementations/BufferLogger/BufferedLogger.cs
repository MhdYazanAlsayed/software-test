using Microsoft.Extensions.Logging;
using ObserveTool.Models;
using ObserveTool.Services.Interfaces;
using System.Diagnostics;

namespace ObserveTool.Services.Implementations.BufferLogger
{
    public sealed class BufferedLogger : ILogger
    {
        private readonly string _category;
        private readonly ILogEntryBuffer _buffer;

        public BufferedLogger(string category, ILogEntryBuffer buffer)
        {
            _category = category;
            _buffer = buffer;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // Avoid entityframework logs
            if (_category.StartsWith("Microsoft.EntityFrameworkCore"))
                return;

            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            _buffer.Enqueue(new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                LevelText = logLevel.ToString(),
                Level = logLevel,
                Category = _category,
                Message = message,
                Exception = exception?.ToString(),
                TraceId = Activity.Current?.TraceId.ToString()
            });
        }
    }
}
