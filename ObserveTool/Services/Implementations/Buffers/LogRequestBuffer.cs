using ObserveTool.Models;
using ObserveTool.Services.Interfaces;
using System.Collections.Concurrent;

namespace ObserveTool.Services.Implementations.Buffers
{
    public class LogRequestBuffer : ILogRequestBuffer
    {
        private readonly ConcurrentQueue<RequestLog> _q = new();

        public List<RequestLog> DequeueBatch(int max)
        {
            var list = new List<RequestLog>(Math.Min(max, 512));
            while (list.Count < max && _q.TryDequeue(out var item))
                list.Add(item);

            return list;
        }

        public void Enqueue(RequestLog entity) => _q.Enqueue(entity);
    }
}
