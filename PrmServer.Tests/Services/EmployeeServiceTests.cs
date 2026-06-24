using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Services
{
    /// <summary>
    /// Unit tests for EmployeeService.
    /// Uses EF Core InMemory database with a Moq IAllocationService.
    /// </summary>
    public class EmployeeServiceTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ISkillRepository> _skillRepoMock;
        private readonly Mock<IAllocationService> _allocationServiceMock;
        private readonly EmployeeService _sut;

        private static int _counter = 0;

        public EmployeeServiceTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase($"EmployeeDb_{System.Threading.Interlocked.Increment(ref _counter)}")
                .Options;

            _db = new PrmDbContext(options);
            _userRepoMock = new Mock<IUserRepository>();
            _skillRepoMock = new Mock<ISkillRepository>();
            _allocationServiceMock = new Mock<IAllocationService>();

            _sut = new EmployeeService(
                _db,
                _userRepoMock.Object,
                _skillRepoMock.Object,
                _allocationServiceMock.Object);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private async Task<User> SeedEngineerAsync(int id, bool isActive = true)
        {
            var role = new Role { Id = 1, RoleName = "Engineer" };
            if (!await _db.Roles.AnyAsync(r => r.Id == 1))
                _db.Roles.Add(role);

            var user = new User
            {
                Id = id, Username = $"eng{id}", FullName = $"Engineer {id}",
                Email = $"eng{id}@test.com", IsActive = isActive,
                PasswordHash = "hash",
                Designation = "Software Engineer", Department = "Engineering",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.UserRoles.Add(new UserRole { UserId = id, RoleId = 1 });
            var status = new UserStatus { UserId = id, Status = "BENCH", UpdatedAt = DateTime.UtcNow };
            _db.UserStatuses.Add(status);
            await _db.SaveChangesAsync();

            // Reload to get navigation properties
            return await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .FirstAsync(u => u.Id == id);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEmployee_WhenEngineerExists()
        {
            await SeedEngineerAsync(id: 1);

            var result = await _sut.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Engineer 1", result!.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserIsNotEngineer()
        {
            // Seed a user with NO engineer role
            _db.Users.Add(new User
            {
                Id = 99, Username = "mgr99", FullName = "Manager 99",
                Email = "mgr@test.com", IsActive = true,
                PasswordHash = "hash",
                Designation = "Manager", Department = "Management",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var result = await _sut.GetByIdAsync(9999);
            Assert.Null(result);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenEmployeeNotFound()
        {
            _userRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((User?)null);

            var dto = new UpdateEmployeeDto
            {
                FullName = "New Name",
                Department = "IT",
                Designation = "Senior Engineer"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.UpdateAsync(999, dto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenUserIsNotAnEngineer()
        {
            var nonEngineerUser = new User
            {
                Id = 50, Username = "admin50", FullName = "Admin User",
                Email = "admin@test.com", IsActive = true,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                // No UserRoles — so Role property returns empty string
                UserRoles = new List<UserRole>()
            };

            _userRepoMock
                .Setup(r => r.GetByIdAsync(50))
                .ReturnsAsync(nonEngineerUser);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.UpdateAsync(50, new UpdateEmployeeDto
                {
                    FullName = "Name", Department = "Dept", Designation = "Dev"
                }));
        }

        // ── DeactivateAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task DeactivateAsync_ShouldThrow_WhenEmployeeNotFound()
        {
            _userRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.DeactivateAsync(999));
        }

        // ── AssignSkillAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AssignSkillAsync_ShouldThrow_WhenSkillAlreadyAssigned()
        {
            var user = await SeedEngineerAsync(id: 5);

            _userRepoMock
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(user);

            var skill = new Skill { Id = 1, Name = "C#", Category = "Backend", CreatedAt = DateTime.UtcNow };
            _db.Skills.Add(skill);
            _db.UserSkills.Add(new UserSkill
            {
                UserId = 5, SkillId = 1, Proficiency = "Expert", AssignedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            _skillRepoMock
                .Setup(r => r.GetByNameAsync("C#"))
                .ReturnsAsync(skill);

            var dto = new AssignSkillDto { SkillName = "C#", Category = "Backend", Proficiency = "Expert" };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AssignSkillAsync(5, dto));

            Assert.Contains("already assigned", ex.Message);
        }

        // ── AssignManagerAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task AssignManagerAsync_ShouldThrow_WhenManagerIsNotManager()
        {
            var engineer = await SeedEngineerAsync(id: 6);
            var nonManager = new User
            {
                Id = 200, Username = "notamgr", FullName = "Not A Manager",
                Email = "notamgr@test.com", IsActive = true,
                PasswordHash = "hash",
                Designation = "Developer", Department = "Engineering",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>()  // no roles
            };

            _userRepoMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(engineer);
            _userRepoMock.Setup(r => r.GetByIdAsync(200)).ReturnsAsync(nonManager);

            var dto = new AssignManagerDto { EmployeeUserId = 6, ManagerUserId = 200 };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AssignManagerAsync(dto));

            Assert.Contains("is not a Manager", ex.Message);
        }

        // ── GetUtilizationAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetUtilizationAsync_ShouldReturnSumOfActiveAllocations()
        {
            _allocationServiceMock
                .Setup(s => s.GetByEmployeeAsync(7))
                .ReturnsAsync(new List<Allocation>
                {
                    new Allocation
                    {
                        UserId = 7, ProjectId = 1, IsActive = true,
                        UtilizationPct = 40,
                        StartDate = DateTime.UtcNow.AddDays(-5),
                        EndDate = DateTime.UtcNow.AddDays(30)
                    },
                    new Allocation
                    {
                        UserId = 7, ProjectId = 2, IsActive = true,
                        UtilizationPct = 30,
                        StartDate = DateTime.UtcNow.AddDays(-5),
                        EndDate = DateTime.UtcNow.AddDays(30)
                    }
                });

            var utilization = await _sut.GetUtilizationAsync(7);

            Assert.Equal(70f, utilization);
        }
    }
}
