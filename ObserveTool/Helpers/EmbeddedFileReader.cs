using System.Reflection;

namespace ObserveTool.Helpers
{
    /// <summary>
    /// Utility class used to read embedded files from the assembly.
    /// 
    /// This is primarily used to load the monitoring dashboard HTML page
    /// that is embedded as a resource inside the library, allowing the
    /// dashboard UI to be served without requiring external static files.
    /// </summary>
    public static class EmbeddedFileReader
    {
        public static string Read(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
                throw new Exception($"Resource {fileName} not found.");

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
