using Microsoft.Extensions.Logging;

namespace ObserveTool.Models
{
    public class LogEntry
    {
        public long Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string LevelText { get; set; } = default!;
        public LogLevel Level { get; set; }
        public string Category { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? Exception { get; set; }

        public string? TraceId { get; set; }           
    }

}
