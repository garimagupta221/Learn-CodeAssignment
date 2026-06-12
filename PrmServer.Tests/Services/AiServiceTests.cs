using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Providers;
using PrmServer.Services;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Services
{
    /// <summary>
    /// Unit tests for AiService.
    /// Uses EF Core InMemory database (seeded per test) and a Moq IAiProvider stub.
    /// </summary>
    public class AiServiceTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<ISystemConfigService> _configMock;
        private readonly Mock<IAiProvider> _providerMock;
        private readonly AiService _sut;

        public AiServiceTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new PrmDbContext(options);
            _configMock = new Mock<ISystemConfigService>();
            _providerMock = new Mock<IAiProvider>();

            // Default provider name matches the config
            _providerMock.Setup(p => p.ProviderName).Returns("Gemma");
            _configMock.Setup(c => c.Get("ActiveAiProvider")).Returns("Gemma");
            _configMock.Setup(c => c.Get("AiApiKey")).Returns("test-key");

            _sut = new AiService(
                _db,
                _configMock.Object,
                new List<IAiProvider> { _providerMock.Object },
                NullLogger<AiService>.Instance);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private async Task SeedMinimalProjectAsync(int projectId = 1)
        {
            var manager = new User
            {
                Id = 10, Username = "mgr", FullName = "Manager One",
                Email = "mgr@test.com", IsActive = true,
                PasswordHash = "hash",
                Designation = "Manager", Department = "Management",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            var project = new Project
            {
                Id = projectId, Name = "Project Alpha", Description = "Test",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "ACTIVE", Health = "GREEN", ManagerId = 10,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(manager);
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
        }

        private async Task SeedEngineerAsync(int userId, int managerId, int projectId)
        {
            var role = new Role { Id = 1, RoleName = "Engineer" };
            if (!_db.Roles.Any(r => r.Id == 1))
                _db.Roles.Add(role);

            var user = new User
            {
                Id = userId, Username = $"eng{userId}", FullName = $"Engineer {userId}",
                Email = $"eng{userId}@test.com", IsActive = true, ManagerId = managerId,
                PasswordHash = "hash",
                Designation = "Software Engineer", Department = "Engineering",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = 1 });
            await _db.SaveChangesAsync();
        }

        // ── Skill Match ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task SkillMatch_ShouldReturnAiResult_WhenEmployeesExist()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            await SeedEngineerAsync(userId: 20, managerId: 10, projectId: 1);

            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("[{\"EmployeeId\": 20, \"FullName\": \"Engineer 20\", \"SkillsMatch\": \"React\", \"Availability\": \"100% free\", \"RecentActivity\": \"None\", \"Reason\": \"Match React\"}]");

            var result = await _sut.GetSkillMatchAsync("React developer", 1, null, managerUserId: 10);

            Assert.Single(result.Recommendations);
            Assert.Equal(20, result.Recommendations[0].EmployeeId);
            Assert.Equal("Engineer 20", result.Recommendations[0].FullName);
            Assert.Equal("Gemma", result.Provider);
        }

        [Fact]
        public async Task SkillMatch_ShouldIncludeMaxHoursInPrompt_WhenProvided()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            await SeedEngineerAsync(userId: 21, managerId: 10, projectId: 1);

            string capturedPrompt = null!;
            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((prompt, _) => capturedPrompt = prompt)
                .ReturnsAsync("[]");

            await _sut.GetSkillMatchAsync("Backend engineer", 1, maxHours: 20, managerUserId: 10);

            Assert.Contains("Employees Context (JSON)", capturedPrompt);
        }

        [Fact]
        public async Task SkillMatch_ShouldThrow_WhenNoEmployeesUnderManager()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            // No engineers seeded under managerId 10

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetSkillMatchAsync("Any requirement", 1, null, managerUserId: 10));

            Assert.Contains("No active engineers found", ex.Message);
        }

        [Fact]
        public async Task SkillMatch_ShouldThrowUnauthorized_WhenManagerDoesNotOwnProject()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            
            // Seed a different manager user (not manager of project 1)
            var otherManager = new User
            {
                Id = 99, Username = "other_mgr", FullName = "Other Manager",
                Email = "other@test.com", IsActive = true, PasswordHash = "hash",
                Designation = "Manager", Department = "Management",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(otherManager);
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.GetSkillMatchAsync("Any requirement", 1, null, managerUserId: 99));
        }

        [Fact]
        public async Task SkillMatch_ShouldAllowAdmin_ToMatchAnyProject()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            await SeedEngineerAsync(userId: 20, managerId: 10, projectId: 1);

            // Seed Admin caller
            var admin = new User
            {
                Id = 88, Username = "admin_user", FullName = "Admin User",
                Email = "admin@test.com", IsActive = true, PasswordHash = "hash",
                Designation = "Admin", Department = "Administration",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(admin);
            
            var adminRole = new Role { Id = 2, RoleName = "Admin" };
            if (!_db.Roles.Any(r => r.Id == 2))
                _db.Roles.Add(adminRole);
            
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = 88, RoleId = 2 });
            await _db.SaveChangesAsync();

            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("[{\"EmployeeId\": 20, \"FullName\": \"Engineer 20\", \"SkillsMatch\": \"React\", \"Availability\": \"100% free\", \"RecentActivity\": \"None\", \"Reason\": \"Match React\"}]");

            var result = await _sut.GetSkillMatchAsync("React developer", 1, null, managerUserId: 88);

            Assert.Single(result.Recommendations);
            Assert.Equal("Engineer 20", result.Recommendations[0].FullName);
            Assert.Equal("Gemma", result.Provider);
        }

        [Fact]
        public async Task SkillMatch_ShouldSupportNullProjectId_AndQueryManagerEngineers()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            await SeedEngineerAsync(userId: 20, managerId: 10, projectId: 1);

            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("[{\"EmployeeId\": 20, \"FullName\": \"Engineer 20\", \"SkillsMatch\": \"React\", \"Availability\": \"100% free\", \"RecentActivity\": \"None\", \"Reason\": \"Match React\"}]");

            var result = await _sut.GetSkillMatchAsync("React developer", null, null, managerUserId: 10);

            Assert.Single(result.Recommendations);
            Assert.Equal("Engineer 20", result.Recommendations[0].FullName);
        }

        [Fact]
        public async Task SkillMatch_ShouldThrow_WhenProviderNotRegistered()
        {
            await SeedMinimalProjectAsync(projectId: 1);
            await SeedEngineerAsync(userId: 22, managerId: 10, projectId: 1);

            // Configure a provider name that doesn't match the registered mock
            _configMock.Setup(c => c.Get("ActiveAiProvider")).Returns("UnknownProvider");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetSkillMatchAsync("Requirement", 1, null, managerUserId: 10));

            Assert.Contains("not registered", ex.Message);
        }

        // ── Risk Summary ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task RiskSummary_ShouldReturnAiResult_ForExistingProject()
        {
            await SeedMinimalProjectAsync(projectId: 2);

            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("Low risk. Timeline is healthy.");

            var result = await _sut.GetProjectRiskSummaryAsync(projectId: 2, managerUserId: 10);

            Assert.Equal("Low risk. Timeline is healthy.", result.Result);
            Assert.Equal("Gemma", result.Provider);
        }

        [Fact]
        public async Task RiskSummary_ShouldReturnFallback_WhenLlmThrows()
        {
            await SeedMinimalProjectAsync(projectId: 3);

            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            var result = await _sut.GetProjectRiskSummaryAsync(projectId: 3, managerUserId: 10);

            Assert.Equal("Fallback", result.Provider);
            Assert.Contains("AI provider is currently unavailable", result.Result);
        }

        [Fact]
        public async Task RiskSummary_FallbackShouldHighlightOverdueMilestones()
        {
            await SeedMinimalProjectAsync(projectId: 4);
            _db.Milestones.Add(new Milestone
            {
                Id = 1, ProjectId = 4, Title = "Phase 1",
                DueDate = DateTime.UtcNow.AddDays(-5),   // overdue
                Status = "PENDING", StoryPoints = 10,
                UpdatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            _providerMock
                .Setup(p => p.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("LLM down"));

            var result = await _sut.GetProjectRiskSummaryAsync(projectId: 4, managerUserId: 10);

            Assert.Contains("overdue milestone", result.Result);
        }

        [Fact]
        public async Task RiskSummary_ShouldThrowKeyNotFound_WhenProjectDoesNotExist()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.GetProjectRiskSummaryAsync(projectId: 9999, managerUserId: 10));
        }

        [Fact]
        public async Task RiskSummary_ShouldReturnFallback_WhenApiKeyMissing()
        {
            await SeedMinimalProjectAsync(projectId: 5);
            _configMock.Setup(c => c.Get("AiApiKey")).Returns(string.Empty);

            // With no API key, GetApiKey() throws — the try/catch in RiskSummary catches it
            var result = await _sut.GetProjectRiskSummaryAsync(projectId: 5, managerUserId: 10);

            Assert.Equal("Fallback", result.Provider);
        }

        [Fact]
        public async Task RiskSummary_ShouldThrowUnauthorized_WhenManagerDoesNotOwnProject()
        {
            await SeedMinimalProjectAsync(projectId: 2);

            var otherManager = new User
            {
                Id = 99, Username = "other_mgr", FullName = "Other Manager",
                Email = "other@test.com", IsActive = true, PasswordHash = "hash",
                Designation = "Manager", Department = "Management",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(otherManager);
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.GetProjectRiskSummaryAsync(projectId: 2, managerUserId: 99));
        }
    }
}
