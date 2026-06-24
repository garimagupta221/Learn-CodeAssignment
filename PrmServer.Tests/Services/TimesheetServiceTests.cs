using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services;
using Xunit;

namespace PrmServer.Tests.Services
{
    /// <summary>
    /// Unit tests for TimesheetService using EF Core InMemory DB.
    /// </summary>
    public class TimesheetServiceTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<ITimesheetRepository> _timesheetRepoMock;
        private readonly Mock<ITimesheetTagRepository> _tagRepoMock;
        private readonly IConfiguration _config;
        private readonly TimesheetService _sut;

        private static int _counter = 0;

        public TimesheetServiceTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase($"TimesheetDb_{System.Threading.Interlocked.Increment(ref _counter)}")
                .Options;

            _db = new PrmDbContext(options);
            _timesheetRepoMock = new Mock<ITimesheetRepository>();
            _tagRepoMock = new Mock<ITimesheetTagRepository>();

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Timesheet:MaxWeeklyHours"] = "60"
                })
                .Build();

            _sut = new TimesheetService(
                _timesheetRepoMock.Object,
                _tagRepoMock.Object,
                _config,
                _db);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private static DateTime LastMonday()
        {
            var today = DateTime.UtcNow.Date;
            int diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            // If today IS Monday, use last week's Monday so it's in the past
            if (diff == 0) diff = 7;
            return today.AddDays(-diff);
        }

        private async Task SeedAllocationAsync(int userId, int projectId, int utilPct = 100)
        {
            _db.Allocations.Add(new Allocation
            {
                UserId = userId, ProjectId = projectId,
                AllocatedBy = 1,
                UtilizationPct = utilPct,
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        // ── ValidateWeekStart ─────────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenWeekStartIsInFuture()
        {
            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 1, ProjectId = 1,
                WeekStart = DateTime.UtcNow.AddDays(7), // future Monday
                HoursLogged = 8, TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("cannot be in the future", ex.Message);
        }

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenWeekStartIsNotMonday()
        {
            // Find the most recent Wednesday (guaranteed not Monday)
            var today = DateTime.UtcNow.Date;
            int daysToWed = ((int)DayOfWeek.Wednesday - (int)today.DayOfWeek + 7) % 7;
            var lastWednesday = today.AddDays(-(7 - daysToWed));

            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 1, ProjectId = 1,
                WeekStart = lastWednesday,
                HoursLogged = 8, TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("must be a Monday", ex.Message);
        }

        // ── ValidateHours ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenHoursAreZero()
        {
            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 1, ProjectId = 1,
                WeekStart = LastMonday(),
                HoursLogged = 0, TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("greater than zero", ex.Message);
        }

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenHoursExceedWeeklyMax()
        {
            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 1, ProjectId = 1,
                WeekStart = LastMonday(),
                HoursLogged = 61, TagIds = new List<int>() // > 60 max
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("weekly maximum", ex.Message);
        }

        // ── ValidateProjectHoursCap ───────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenNoActiveAllocationExists()
        {
            // No allocation seeded → should fail with "No active allocation found"
            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 1, ProjectId = 1,
                WeekStart = LastMonday(),
                HoursLogged = 8, TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("No active allocation found", ex.Message);
        }

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenHoursExceedProjectCap()
        {
            // 50% allocation × 60 max = 30 hr cap
            await SeedAllocationAsync(userId: 2, projectId: 2, utilPct: 50);

            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 2, ProjectId = 2,
                WeekStart = LastMonday(),
                HoursLogged = 31, // exceeds 30hr cap
                TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("project cap", ex.Message);
        }

        // ── Duplicate check ───────────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenDuplicateTimesheetExists()
        {
            await SeedAllocationAsync(userId: 3, projectId: 3, utilPct: 100);

            // Pre-seed an existing timesheet for the same week/project
            var monday = LastMonday();
            _db.Timesheets.Add(new Timesheet
            {
                UserId = 3, ProjectId = 3,
                WeekStart = monday,
                HoursLogged = 20, Status = "SUBMITTED",
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 3, ProjectId = 3,
                WeekStart = monday,
                HoursLogged = 20, TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("already been submitted", ex.Message);
        }

        // ── Happy path ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAsync_ShouldCallRepository_WhenValidDtoProvided()
        {
            await SeedAllocationAsync(userId: 4, projectId: 4, utilPct: 100);

            var createdTimesheet = new Timesheet
            {
                Id = 1, UserId = 4, ProjectId = 4,
                WeekStart = LastMonday(), HoursLogged = 30,
                Status = "SUBMITTED", SubmittedAt = DateTime.UtcNow,
                TimesheetTags = new List<TimesheetTag>()
            };

            _timesheetRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Timesheet>()))
                .ReturnsAsync(createdTimesheet);

            _timesheetRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(createdTimesheet);

            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 4, ProjectId = 4,
                WeekStart = LastMonday(),
                HoursLogged = 30, TagIds = new List<int>()
            };

            var result = await _sut.SubmitAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("SUBMITTED", result.Status);
            _timesheetRepoMock.Verify(r => r.AddAsync(It.IsAny<Timesheet>()), Times.Once);
        }

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenEmployeeTimesheetAccessIsFrozen()
        {
            // Seed a frozen user
            var frozenUser = new User
            {
                Id = 5,
                Username = "frozen.test",
                Email = "frozen@prm.com",
                FullName = "Frozen Employee",
                TimesheetAccessFrozen = true,
                Department = "Dev",
                Designation = "SE",
                PasswordHash = "dummy_hash"
            };
            _db.Users.Add(frozenUser);
            await _db.SaveChangesAsync();

            var dto = new SubmitTimesheetDto
            {
                EmployeeId = 5,
                ProjectId = 1,
                WeekStart = LastMonday(),
                HoursLogged = 8,
                TagIds = new List<int>()
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.SubmitAsync(dto));

            Assert.Contains("Your timesheet access has been frozen", ex.Message);
        }
    }
}
