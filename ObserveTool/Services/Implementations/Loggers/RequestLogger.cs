using ObserveTool.Models;
using ObserveTool.Services.Interfaces;

namespace ObserveTool.Services.Implementations.Loggers
{
    public class RequestLogger : IRequestLogger
    {
        private readonly ILogRequestBuffer _logRequestBuffer;

        public RequestLogger(ILogRequestBuffer logRequestBuffer)
        {
            _logRequestBuffer = logRequestBuffer;
        }

        public void Log(RequestLog log)
        {
            _logRequestBuffer.Enqueue(log);
        }
    }
}
