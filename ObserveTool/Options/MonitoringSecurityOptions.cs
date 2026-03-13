namespace ObserveTool.Options
{
    public class MonitoringSecurityOptions
    {
        public bool EnableAuth { get; set; } = true;
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "MyP@ssw0rd";
    }
}
