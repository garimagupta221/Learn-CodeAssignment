using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services;
using Xunit;

namespace PrmServer.Tests.Services
{
    /// <summary>
    /// Unit tests for AllocationService.
    /// Uses EF Core InMemory database for the parts that hit _context directly,
    /// and Moq for the IAllocationRepository.
    /// </summary>
    public class AllocationServiceTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<IAllocationRepository> _allocationRepoMock;
        private readonly AllocationService _sut;

        private static int _dbCounter = 0;

        public AllocationServiceTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase(databaseName: $"AllocationDb_{System.Threading.Interlocked.Increment(ref _dbCounter)}")
                .Options;

            _db = new PrmDbContext(options);
            _allocationRepoMock = new Mock<IAllocationRepository>();

            _sut = new AllocationService(_allocationRepoMock.Object, _db);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private async Task<User> SeedUserAsync(int id, bool isActive = true)
        {
            var user = new User
            {
                Id = id, Username = $"user{id}", FullName = $"User {id}",
                Email = $"user{id}@test.com", IsActive = isActive,
                PasswordHash = "hash",
                Designation = "Engineer", Department = "Engineering",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        private async Task<Project> SeedProjectAsync(int id, string status = "ACTIVE")
        {
            var manager = _db.Users.Find(10)
                ?? (await SeedUserAsync(10));

            var project = new Project
            {
                Id = id, Name = $"Project {id}", Description = "Test",
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(90),
                Status = status, ManagerId = 10,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        // ── Validation guard tests ────────────────────────────────────────────────────

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenUtilizationIsZero()
        {
            var dto = new CreateAllocationDto
            {
                EmployeeId = 1, ProjectId = 1,
                UtilizationPct = 0,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.AllocateAsync(dto));

            Assert.Contains("Utilization must be between 1 and 100", ex.Message);
        }

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenUtilizationExceeds100()
        {
            var dto = new CreateAllocationDto
            {
                EmployeeId = 1, ProjectId = 1,
                UtilizationPct = 101,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AllocateAsync(dto));
        }

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenStartDateAfterEndDate()
        {
            var dto = new CreateAllocationDto
            {
                EmployeeId = 1, ProjectId = 1,
                UtilizationPct = 50,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.AllocateAsync(dto));

            Assert.Contains("Start date must be before end date", ex.Message);
        }

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenProjectNotFound()
        {
            await SeedUserAsync(id: 5);

            var dto = new CreateAllocationDto
            {
                EmployeeId = 5, ProjectId = 999, // non-existent project
                UtilizationPct = 50,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.AllocateAsync(dto));
        }

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenEmployeeNotFound()
        {
            await SeedProjectAsync(id: 1);

            var dto = new CreateAllocationDto
            {
                EmployeeId = 999, ProjectId = 1, // non-existent employee
                UtilizationPct = 50,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.AllocateAsync(dto));
        }

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenEmployeeIsInactive()
        {
            await SeedUserAsync(id: 6, isActive: false);
            await SeedProjectAsync(id: 2);

            var dto = new CreateAllocationDto
            {
                EmployeeId = 6, ProjectId = 2,
                UtilizationPct = 50,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AllocateAsync(dto));

            Assert.Contains("deactivated", ex.Message);
        }

        [Fact]
        public async Task AllocateAsync_ShouldThrow_WhenProjectIsCompleted()
        {
            await SeedUserAsync(id: 7);
            await SeedProjectAsync(id: 3, status: "COMPLETED");

            var dto = new CreateAllocationDto
            {
                EmployeeId = 7, ProjectId = 3,
                UtilizationPct = 50,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AllocateAsync(dto));

            Assert.Contains("ACTIVE or PLANNED", ex.Message);
        }

        // ── Over-allocation guard ─────────────────────────────────────────────────────

        [Fact]
        public async Task IsOverAllocatedAsync_ShouldReturnTrue_WhenSumExceeds100()
        {
            var existingAllocations = new List<Allocation>
            {
                new Allocation
                {
                    UserId = 1, ProjectId = 1, IsActive = true,
                    UtilizationPct = 70,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(30)
                }
            };

            _allocationRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(existingAllocations);

            // Adding 40% on top of existing 70% = 110% — over-allocated
            var isOver = await _sut.IsOverAllocatedAsync(
                1, 40, DateTime.UtcNow, DateTime.UtcNow.AddDays(20));

            Assert.True(isOver);
        }

        [Fact]
        public async Task IsOverAllocatedAsync_ShouldReturnFalse_WhenSumIsExactly100()
        {
            var existingAllocations = new List<Allocation>
            {
                new Allocation
                {
                    UserId = 1, ProjectId = 1, IsActive = true,
                    UtilizationPct = 60,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(30)
                }
            };

            _allocationRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(existingAllocations);

            // 60 + 40 = 100 — exactly at limit, NOT over-allocated
            var isOver = await _sut.IsOverAllocatedAsync(
                1, 40, DateTime.UtcNow, DateTime.UtcNow.AddDays(20));

            Assert.False(isOver);
        }

        [Fact]
        public async Task IsOverAllocatedAsync_ShouldReturnFalse_WhenAllocationsDoNotOverlap()
        {
            var existingAllocations = new List<Allocation>
            {
                new Allocation
                {
                    UserId = 2, ProjectId = 1, IsActive = true,
                    UtilizationPct = 90,
                    // Past allocation — does not overlap with the new range
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            _allocationRepoMock
                .Setup(r => r.GetByUserIdAsync(2))
                .ReturnsAsync(existingAllocations);

            var isOver = await _sut.IsOverAllocatedAsync(
                2, 90, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(30));

            Assert.False(isOver);
        }

        // ── EndAllocation ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task EndAllocationAsync_ShouldThrow_WhenNotFound()
        {
            _allocationRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Allocation?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.EndAllocationAsync(999));
        }
    }
}
