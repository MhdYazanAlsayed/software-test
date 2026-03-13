namespace ObserveTool.Options
{
    public sealed class MonitoringOptions
    {
        public string Schema { get; set; } = "monitoring";
        public int BatchSize { get; set; } = 200;
        public int FlushIntervalSeconds { get; set; } = 30;
        public bool AutoMigrateOnStartup { get; set; } = true;
        public bool EnableRequestLogging { get; set; } = true;
        public MonitoringSecurityOptions Security { get; set; } = new();
    }
}
