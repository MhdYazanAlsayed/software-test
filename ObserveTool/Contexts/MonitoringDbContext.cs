using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObserveTool.Models;
using ObserveTool.Options;

namespace ObserveTool.Contexts
{
    /// <summary>
    /// EF Core context used to store application monitoring logs,
    /// including system logs and HTTP request/response logs.
    /// </summary>
    public sealed class MonitoringDbContext : DbContext
    {
        private readonly MonitoringOptions _options;

        public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options, IOptions<MonitoringOptions> opt)
            : base(options) => _options = opt.Value;

        public DbSet<LogEntry> Logs => Set<LogEntry>();
        public DbSet<RequestLog> RequestLogs => Set<RequestLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_options.Schema);

            modelBuilder.Entity<LogEntry>(b =>
            {
                b.ToTable("Logs"); // schema = default above
                b.HasKey(x => x.Id);

                b.Property(x => x.LevelText).HasMaxLength(20);
                b.Property(x => x.Category).HasMaxLength(200);
                b.Property(x => x.TraceId).HasMaxLength(64);

                b.HasIndex(x => x.Timestamp);
                b.HasIndex(x => x.Level);
                b.HasIndex(x => x.TraceId);
            });

            modelBuilder.Entity<RequestLog>(b =>
            {
                b.ToTable("RequestLogs"); // schema = default above
                b.HasKey(x => x.Id);

                b.Property(x => x.Path).HasMaxLength(500);
                b.Property(x => x.Method).HasMaxLength(20);
                b.Property(x => x.Request).HasColumnType("NVARCHAR(MAX)");
                b.Property(x => x.Response).HasColumnType("NVARCHAR(MAX)");
                b.Property(x => x.IpAddress).HasColumnType("NVARCHAR(100)");

                b.HasIndex(x => x.Timestamp);
                b.HasIndex(x => x.Method);
            });
        }
    }
}
