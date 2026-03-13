using ObserveTool.Models.Enums;

namespace ObserveTool.Models
{
    public class RequestLog
    {
        public RequestLog()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public int Id { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string? Path { get; set; }
        public RequestMethod? Method { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }
        public string? IpAddress { get; set; }
        public string? TraceId { get; set; }
        public int? StatusCode { get; set; }
    }
}
