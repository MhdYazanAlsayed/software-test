namespace ObserveTool.Services.Interfaces
{
    /// <summary>
    /// Represents a generic in-memory buffer used to temporarily store log entities
    /// before they are persisted to the database.
    ///
    /// The buffer supports enqueueing new log items and dequeuing them in batches
    /// to enable efficient background processing and batch inserts.
    /// </summary>
    public interface ILogBuffer<T>
    {
        void Enqueue(T entity);
        List<T> DequeueBatch(int max);
    }
}
