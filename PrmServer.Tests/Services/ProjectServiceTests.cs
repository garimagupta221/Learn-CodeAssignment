using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services;
using Xunit;

namespace PrmServer.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IMilestoneRepository> _milestoneRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _milestoneRepositoryMock = new Mock<IMilestoneRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _projectService = new ProjectService(
                _projectRepositoryMock.Object,
                _milestoneRepositoryMock.Object,
                _userRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_IfStartDateAfterEndDate()
        {
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.CreateAsync(dto));
            Assert.Equal("Start date must be before end date.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_IfUserIsNotManager()
        {
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                ManagerId = 1
            };

            var nonManagerUser = new User
            {
                Id = 1,
                Username = "test.employee",
                Designation = "Employee" // Role string returns Designation if UserRoles empty
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(dto.ManagerId))
                .ReturnsAsync(nonManagerUser);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _projectService.CreateAsync(dto));
            Assert.Contains("is not a Manager", ex.Message);
        }

        [Fact]
        public async Task GetHealthAsync_ShouldReturnRed_IfOverdue()
        {
            var projectId = 1;
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Status = "PENDING", DueDate = DateTime.UtcNow.AddDays(-2) }
            };

            _milestoneRepositoryMock.Setup(repo => repo.GetByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            var result = await _projectService.GetHealthAsync(projectId);

            Assert.Equal("RED", result);
        }

        [Fact]
        public async Task GetHealthAsync_ShouldReturnAmber_IfApproachingDeadline()
        {
            var projectId = 1;
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Status = "PENDING", DueDate = DateTime.UtcNow.AddDays(3) }
            };

            _milestoneRepositoryMock.Setup(repo => repo.GetByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            var result = await _projectService.GetHealthAsync(projectId);

            Assert.Equal("AMBER", result);
        }

        [Fact]
        public async Task GetHealthAsync_ShouldReturnGreen_IfNoApproachingDeadline()
        {
            var projectId = 1;
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Status = "PENDING", DueDate = DateTime.UtcNow.AddDays(10) }
            };

            _milestoneRepositoryMock.Setup(repo => repo.GetByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            var result = await _projectService.GetHealthAsync(projectId);

            Assert.Equal("GREEN", result);
        }

        [Fact]
        public async Task AddMilestoneAsync_ShouldThrow_IfStoryPointsExceedTotal()
        {
            var projectId = 1;
            var dto = new AddMilestoneDto { StoryPoints = 50 };

            var project = new Project { Id = projectId, TotalStoryPoints = 100 };
            var existingMilestones = new List<Milestone>
            {
                new Milestone { StoryPoints = 60 }
            };

            _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(projectId))
                .ReturnsAsync(project);
            _milestoneRepositoryMock.Setup(repo => repo.GetByProjectIdAsync(projectId))
                .ReturnsAsync(existingMilestones);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _projectService.AddMilestoneAsync(projectId, dto));
            Assert.Contains("Milestone points cannot exceed the project's total remaining points", ex.Message);
        }
    }
}
