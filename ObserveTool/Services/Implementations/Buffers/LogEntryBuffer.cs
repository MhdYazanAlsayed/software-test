using ObserveTool.Models;
using ObserveTool.Services.Interfaces;
using System.Collections.Concurrent;

namespace ObserveTool.Services.Implementations.Buffers
{
    public sealed class LogEntryBuffer : ILogEntryBuffer
    {
        private readonly ConcurrentQueue<LogEntry> _q = new();

        public void Enqueue(LogEntry entry) => _q.Enqueue(entry);

        public List<LogEntry> DequeueBatch(int max)
        {
            var list = new List<LogEntry>(Math.Min(max, 512));
            while (list.Count < max && _q.TryDequeue(out var item))
                list.Add(item);
            return list;
        }
    }
}
