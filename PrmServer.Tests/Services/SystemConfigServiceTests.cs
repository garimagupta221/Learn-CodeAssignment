using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PrmServer.Entities;
using PrmServer.Services;
using Xunit;

namespace PrmServer.Tests.Services
{
    public class SystemConfigServiceTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly SystemConfigService _sut;

        public SystemConfigServiceTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new PrmDbContext(options);

            // Mock IServiceScopeFactory and IServiceScope to return our db context
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(PrmDbContext)))
                .Returns(_db);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock
                .Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);

            _sut = new SystemConfigService(_scopeFactoryMock.Object);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public void Get_ShouldReturnValue_WhenKeyExists()
        {
            _db.SystemConfigs.Add(new SystemConfig { Key = "TestKey", Value = "TestValue" });
            _db.SaveChanges();

            var result = _sut.Get("TestKey");

            Assert.Equal("TestValue", result);
        }

        [Fact]
        public void Get_ShouldReturnEmptyString_WhenKeyDoesNotExist()
        {
            var result = _sut.Get("NonExistent");

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Set_ShouldCreateNewConfig_WhenKeyDoesNotExist()
        {
            _sut.Set("NewKey", "NewValue");

            var config = _db.SystemConfigs.FirstOrDefault(c => c.Key == "NewKey");
            Assert.NotNull(config);
            Assert.Equal("NewValue", config.Value);
        }

        [Fact]
        public void Set_ShouldUpdateExistingConfig_WhenKeyExists()
        {
            _db.SystemConfigs.Add(new SystemConfig { Key = "ExistKey", Value = "OldValue" });
            _db.SaveChanges();

            _sut.Set("ExistKey", "NewValue");

            var config = _db.SystemConfigs.FirstOrDefault(c => c.Key == "ExistKey");
            Assert.NotNull(config);
            Assert.Equal("NewValue", config.Value);
        }

        [Fact]
        public void GetAll_ShouldReturnAllConfigurations()
        {
            _db.SystemConfigs.AddRange(new[]
            {
                new SystemConfig { Key = "Key1", Value = "Val1" },
                new SystemConfig { Key = "Key2", Value = "Val2" }
            });
            _db.SaveChanges();

            var result = _sut.GetAll();

            Assert.Equal(2, result.Count);
            Assert.Equal("Val1", result["Key1"]);
            Assert.Equal("Val2", result["Key2"]);
        }
    }
}
