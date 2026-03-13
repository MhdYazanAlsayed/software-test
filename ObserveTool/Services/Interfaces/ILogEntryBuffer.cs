using ObserveTool.Models;

namespace ObserveTool.Services.Interfaces
{
    /// <summary>
    /// Specialized buffer for application log entries.
    /// </summary>
    public interface ILogEntryBuffer : ILogBuffer<LogEntry>
    {
    }
}
