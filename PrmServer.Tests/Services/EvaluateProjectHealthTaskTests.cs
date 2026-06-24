using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.BackgroundTasks;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Services
{
    public class EvaluateProjectHealthTaskTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IAiService> _aiServiceMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly EvaluateProjectHealthTask _sut;

        private static int _counter = 0;

        public EvaluateProjectHealthTaskTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase($"EvaluateProjectHealthDb_{System.Threading.Interlocked.Increment(ref _counter)}")
                .Options;

            _db = new PrmDbContext(options);
            _projectRepoMock = new Mock<IProjectRepository>();
            _projectServiceMock = new Mock<IProjectService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _emailServiceMock = new Mock<IEmailService>();
            _aiServiceMock = new Mock<IAiService>();

            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(PrmDbContext))).Returns(_db);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IProjectRepository))).Returns(_projectRepoMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IProjectService))).Returns(_projectServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationService))).Returns(_notificationServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmailService))).Returns(_emailServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IAiService))).Returns(_aiServiceMock.Object);

            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            _sut = new EvaluateProjectHealthTask(NullLogger<EvaluateProjectHealthTask>.Instance);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public async Task ExecuteAsync_ShouldSendAlert_WhenHealthDegradesFromGreenToRed()
        {
            // Arrange
            var manager = new User
            {
                Id = 10,
                Username = "mgr.bhavika",
                Email = "bhavika.patel@prm.test",
                FullName = "Bhavika Patel",
                Designation = "Manager",
                Department = "Dev",
                PasswordHash = "dummy_hash",
                UserRoles = new List<UserRole> { new UserRole { Role = new Role { RoleName = "Manager" } } }
            };
            _db.Users.Add(manager);

            var project = new Project
            {
                Id = 1,
                Name = "HP fintech",
                Description = "HP fintech description",
                Status = "ACTIVE",
                Health = "GREEN",
                ManagerId = manager.Id,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10)
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            _projectRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Project> { project });

            _projectServiceMock.Setup(s => s.GetHealthAsync(project.Id))
                .ReturnsAsync("RED");

            // Mock AI call fallbacks/results
            var aiResult = new AiResult("AI Risk Summary text goes here", "Gemini");
            _aiServiceMock.Setup(a => a.GetProjectRiskSummaryAsync(project.Id, manager.Id))
                .ReturnsAsync(aiResult);

            var skillMatch = new SkillMatchResult(new List<SkillMatchRecommendationDto>(), "Gemini");
            _aiServiceMock.Setup(a => a.GetSkillMatchAsync(It.IsAny<string>(), project.Id, null, manager.Id))
                .ReturnsAsync(skillMatch);

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            _projectRepoMock.Verify(r => r.UpdateAsync(It.Is<Project>(p => p.Health == "RED")), Times.Once);

            _notificationServiceMock.Verify(n => n.CreateAsync(
                manager.Id,
                It.Is<string>(s => s.Contains("health has changed from GREEN to RED")),
                "PROJECT_HEALTH"), Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync(
                manager.Email, manager.FullName,
                "[RED] Project at Risk: HP fintech",
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotSendAlert_WhenHealthIsUnchanged()
        {
            // Arrange
            var project = new Project
            {
                Id = 2,
                Name = "BasicWash",
                Description = "BasicWash description",
                Status = "ACTIVE",
                Health = "RED"
            };

            _projectRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Project> { project });

            _projectServiceMock.Setup(s => s.GetHealthAsync(project.Id))
                .ReturnsAsync("RED");

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            _projectRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Never);
            _notificationServiceMock.Verify(n => n.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotSendAlert_WhenHealthImproves()
        {
            // Arrange
            var project = new Project
            {
                Id = 3,
                Name = "BasicWash",
                Description = "BasicWash description",
                Status = "ACTIVE",
                Health = "RED"
            };

            _projectRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Project> { project });

            _projectServiceMock.Setup(s => s.GetHealthAsync(project.Id))
                .ReturnsAsync("GREEN");

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            // Project status should still be updated in DB
            _projectRepoMock.Verify(r => r.UpdateAsync(It.Is<Project>(p => p.Health == "GREEN")), Times.Once);

            // But no emails or notifications are sent since it's an improvement
            _notificationServiceMock.Verify(n => n.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendAlert_WhenHealthDegradesFromGreenToAmber()
        {
            // Arrange
            var manager = new User
            {
                Id = 10,
                Username = "mgr.bhavika",
                Email = "bhavika.patel@prm.test",
                FullName = "Bhavika Patel",
                Designation = "Manager",
                Department = "Dev",
                PasswordHash = "dummy_hash",
                UserRoles = new List<UserRole> { new UserRole { Role = new Role { RoleName = "Manager" } } }
            };
            _db.Users.Add(manager);

            var project = new Project
            {
                Id = 4,
                Name = "HP fintech",
                Description = "HP fintech description",
                Status = "ACTIVE",
                Health = "GREEN",
                ManagerId = manager.Id,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10)
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            _projectRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Project> { project });

            _projectServiceMock.Setup(s => s.GetHealthAsync(project.Id))
                .ReturnsAsync("AMBER");

            // Mock AI call fallbacks/results
            var aiResult = new AiResult("AI Risk Summary text goes here", "Gemini");
            _aiServiceMock.Setup(a => a.GetProjectRiskSummaryAsync(project.Id, manager.Id))
                .ReturnsAsync(aiResult);

            var skillMatch = new SkillMatchResult(new List<SkillMatchRecommendationDto>(), "Gemini");
            _aiServiceMock.Setup(a => a.GetSkillMatchAsync(It.IsAny<string>(), project.Id, null, manager.Id))
                .ReturnsAsync(skillMatch);

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            _projectRepoMock.Verify(r => r.UpdateAsync(It.Is<Project>(p => p.Health == "AMBER")), Times.Once);

            _notificationServiceMock.Verify(n => n.CreateAsync(
                manager.Id,
                It.Is<string>(s => s.Contains("health has changed from GREEN to AMBER")),
                "PROJECT_HEALTH"), Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync(
                manager.Email, manager.FullName,
                "[AMBER] Project at Risk: HP fintech",
                It.IsAny<string>()), Times.Once);
        }
    }
}
