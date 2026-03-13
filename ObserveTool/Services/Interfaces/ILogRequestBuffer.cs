using ObserveTool.Models;

namespace ObserveTool.Services.Interfaces
{
    /// <summary>
    /// Specialized buffer for HTTP request log entries.
    /// </summary>
    public interface ILogRequestBuffer : ILogBuffer<RequestLog>
    {
    }
}
