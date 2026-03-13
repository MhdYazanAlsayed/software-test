using ObserveTool.Models;

namespace ObserveTool.Services.Interfaces
{
    /// <summary>
    /// Request logger provider interface.
    /// Used to consume ILogRequestBuffer.
    /// </summary>
    public interface IRequestLogger
    {
        void Log(RequestLog log);
    }
}
