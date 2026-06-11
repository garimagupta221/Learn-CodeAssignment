using System.Text.Json;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly string _filePath;
        private readonly object _lock = new();

        public SystemConfigService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var relativePath = configuration["SystemConfig:FilePath"] ?? "system_config.json";
            _filePath = Path.Combine(environment.ContentRootPath, relativePath);

            EnsureFileExists();
        }

        public string Get(string key)
        {
            var config = ReadFromFile();
            config.TryGetValue(key, out var value);
            return value ?? string.Empty;
        }

        public void Set(string key, string value)
        {
            lock (_lock)
            {
                var config = ReadFromFile();
                config[key] = value;
                WriteToFile(config);
            }
        }

        public Dictionary<string, string> GetAll()
        {
            return ReadFromFile();
        }

        // --- Private helpers ---

        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
                WriteToFile(new Dictionary<string, string>());
        }

        private Dictionary<string, string> ReadFromFile()
        {
            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }

        private void WriteToFile(Dictionary<string, string> config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }
    }
}
