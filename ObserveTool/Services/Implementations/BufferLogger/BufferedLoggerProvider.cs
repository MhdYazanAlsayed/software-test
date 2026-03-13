using Microsoft.Extensions.Logging;
using ObserveTool.Services.Interfaces;

namespace ObserveTool.Services.Implementations.BufferLogger
{
    public sealed class BufferedLoggerProvider : ILoggerProvider
    {
        private readonly ILogEntryBuffer _buffer;

        public BufferedLoggerProvider(ILogEntryBuffer buffer) => _buffer = buffer;

        public ILogger CreateLogger(string categoryName) => new BufferedLogger(categoryName, _buffer);

        public void Dispose() { }
    }
}
