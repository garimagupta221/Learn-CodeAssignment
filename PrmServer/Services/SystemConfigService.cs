using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PrmServer.Entities;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SystemConfigService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public string Get(string key)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
            var config = db.SystemConfigs.FirstOrDefault(c => c.Key == key);
            return config?.Value ?? string.Empty;
        }

        public void Set(string key, string value)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
            var config = db.SystemConfigs.FirstOrDefault(c => c.Key == key);
            if (config == null)
            {
                config = new SystemConfig { Key = key, Value = value };
                db.SystemConfigs.Add(config);
            }
            else
            {
                config.Value = value;
            }
            db.SaveChanges();
        }

        public Dictionary<string, string> GetAll()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
            return db.SystemConfigs.ToDictionary(c => c.Key, c => c.Value);
        }
    }
}
