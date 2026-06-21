using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PrmServer.Controllers;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Controllers
{
    public class UsersControllerTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IAllocationService> _allocationServiceMock;
        private readonly UsersController _controller;

        private static int _counter = 0;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase($"UsersControllerDb_{System.Threading.Interlocked.Increment(ref _counter)}")
                .Options;

            _db = new PrmDbContext(options);
            _userRepoMock = new Mock<IUserRepository>();
            _allocationServiceMock = new Mock<IAllocationService>();

            _controller = new UsersController(
                _userRepoMock.Object,
                _db,
                _allocationServiceMock.Object);

            // Seed standard Roles
            _db.Roles.AddRange(
                new Role { Id = 1, RoleName = "Admin" },
                new Role { Id = 2, RoleName = "Manager" },
                new Role { Id = 3, RoleName = "Engineer" }
            );
            _db.SaveChanges();
        }

        public void Dispose() => _db.Dispose();

        private void SetCallerContext(int userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // ── GetFreezeStatus Tests ───────────────────────────────────────────────────

        [Fact]
        public async Task GetFreezeStatus_ReturnsOk_WithCorrectFreezeStatus()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "test.user",
                Email = "test@prm.com",
                FullName = "Test User",
                TimesheetAccessFrozen = true,
                Department = "Dev",
                Designation = "SE",
                PasswordHash = "dummy"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.GetFreezeStatus(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var userIdProp = value.GetType().GetProperty("userId");
            var isFrozenProp = value.GetType().GetProperty("isTimesheetFrozen");

            Assert.NotNull(userIdProp);
            Assert.NotNull(isFrozenProp);
            Assert.Equal(1, userIdProp.GetValue(value));
            Assert.True((bool)isFrozenProp.GetValue(value));
        }

        [Fact]
        public async Task GetFreezeStatus_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.GetFreezeStatus(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User 999 not found.", notFoundResult.Value);
        }

        // ── GetFrozenEmployees Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task GetFrozenEmployees_ReturnsUnauthorized_WhenCallerClaimIsMissing()
        {
            // Arrange (Empty HttpContext/User)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetFrozenEmployees();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetFrozenEmployees_ReturnsFrozenDirectReports_ForManager()
        {
            // Arrange
            SetCallerContext(10); // Manager Bhavika (Id = 10)

            var manager = new User
            {
                Id = 10,
                Username = "mgr.bhavika",
                Email = "bhavika@prm.test",
                FullName = "Bhavika Patel",
                Designation = "Manager",
                Department = "Dev",
                PasswordHash = "dummy"
            };
            manager.UserRoles = new List<UserRole> { new UserRole { UserId = 10, RoleId = 2 } };

            var report1 = new User
            {
                Id = 11,
                Username = "eng.rohit",
                Email = "rohit@prm.test",
                FullName = "Rohit Verma",
                TimesheetAccessFrozen = true,
                ManagerId = 10,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            var report2 = new User
            {
                Id = 12,
                Username = "eng.arshia",
                Email = "arshia@prm.test",
                FullName = "Arshia Sen",
                TimesheetAccessFrozen = false, // Not frozen
                ManagerId = 10,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            var otherFrozen = new User
            {
                Id = 13,
                Username = "eng.kavita",
                Email = "kavita@prm.test",
                FullName = "Kavita Reddy",
                TimesheetAccessFrozen = true, // Frozen, but reports to manager ID 20
                ManagerId = 20,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            _db.Users.AddRange(manager, report1, report2, otherFrozen);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.GetFrozenEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value).ToList();

            Assert.Single(list);
            var item = list.First();
            var userIdProp = item.GetType().GetProperty("userId");
            Assert.NotNull(userIdProp);
            Assert.Equal(11, userIdProp.GetValue(item));
        }

        [Fact]
        public async Task GetFrozenEmployees_ReturnsAllFrozenUsers_ForAdmin()
        {
            // Arrange
            SetCallerContext(99); // Admin (Id = 99)

            var admin = new User
            {
                Id = 99,
                Username = "admin.user",
                Email = "admin@prm.test",
                FullName = "System Admin",
                Designation = "Admin",
                Department = "IT",
                PasswordHash = "dummy"
            };
            admin.UserRoles = new List<UserRole> { new UserRole { UserId = 99, RoleId = 1 } };

            var report1 = new User
            {
                Id = 11,
                Username = "eng.rohit",
                Email = "rohit@prm.test",
                FullName = "Rohit Verma",
                TimesheetAccessFrozen = true,
                ManagerId = 10,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            var otherFrozen = new User
            {
                Id = 13,
                Username = "eng.kavita",
                Email = "kavita@prm.test",
                FullName = "Kavita Reddy",
                TimesheetAccessFrozen = true,
                ManagerId = 20,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            _db.Users.AddRange(admin, report1, otherFrozen);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.GetFrozenEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value).ToList();

            Assert.Equal(2, list.Count);
        }

        // ── UnfreezeTimesheet Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task UnfreezeTimesheet_ReturnsOk_WhenManagerUnfreezesDirectReport()
        {
            // Arrange
            SetCallerContext(10); // Manager (Id = 10)

            var manager = new User
            {
                Id = 10,
                Username = "mgr.bhavika",
                Email = "bhavika@prm.test",
                FullName = "Bhavika Patel",
                Designation = "Manager",
                Department = "Dev",
                PasswordHash = "dummy"
            };
            manager.UserRoles = new List<UserRole> { new UserRole { UserId = 10, RoleId = 2 } };

            var report = new User
            {
                Id = 11,
                Username = "eng.rohit",
                Email = "rohit@prm.test",
                FullName = "Rohit Verma",
                TimesheetAccessFrozen = true,
                ManagerId = 10,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            _db.Users.AddRange(manager, report);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.UnfreezeTimesheet(11);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var msgProp = value.GetType().GetProperty("message");
            Assert.NotNull(msgProp);
            Assert.Contains("access has been restored", msgProp.GetValue(value) as string);

            // Verify database update
            var updatedUser = await _db.Users.FindAsync(11);
            Assert.NotNull(updatedUser);
            Assert.False(updatedUser.TimesheetAccessFrozen);
        }

        [Fact]
        public async Task UnfreezeTimesheet_ReturnsForbid_WhenManagerUnfreezesNonReport()
        {
            // Arrange
            SetCallerContext(10); // Manager (Id = 10)

            var manager = new User
            {
                Id = 10,
                Username = "mgr.bhavika",
                Email = "bhavika@prm.test",
                FullName = "Bhavika Patel",
                Designation = "Manager",
                Department = "Dev",
                PasswordHash = "dummy"
            };
            manager.UserRoles = new List<UserRole> { new UserRole { UserId = 10, RoleId = 2 } };

            var nonReport = new User
            {
                Id = 13,
                Username = "eng.kavita",
                Email = "kavita@prm.test",
                FullName = "Kavita Reddy",
                TimesheetAccessFrozen = true,
                ManagerId = 20, // Reports to manager ID 20
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            _db.Users.AddRange(manager, nonReport);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.UnfreezeTimesheet(13);

            // Assert
            Assert.IsType<ForbidResult>(result);

            // Verify user is still frozen
            var updatedUser = await _db.Users.FindAsync(13);
            Assert.NotNull(updatedUser);
            Assert.True(updatedUser.TimesheetAccessFrozen);
        }

        [Fact]
        public async Task UnfreezeTimesheet_ReturnsForbid_WhenEngineerAttemptsUnfreeze()
        {
            // Arrange
            SetCallerContext(11); // Engineer (Id = 11)

            var engineer = new User
            {
                Id = 11,
                Username = "eng.rohit",
                Email = "rohit@prm.test",
                FullName = "Rohit Verma",
                TimesheetAccessFrozen = true,
                ManagerId = 10,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };
            engineer.UserRoles = new List<UserRole> { new UserRole { UserId = 11, RoleId = 3 } };

            _db.Users.Add(engineer);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.UnfreezeTimesheet(11);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UnfreezeTimesheet_ReturnsBadRequest_WhenUserIsNotFrozen()
        {
            // Arrange
            SetCallerContext(10); // Manager (Id = 10)

            var manager = new User
            {
                Id = 10,
                Username = "mgr.bhavika",
                Email = "bhavika@prm.test",
                FullName = "Bhavika Patel",
                Designation = "Manager",
                Department = "Dev",
                PasswordHash = "dummy"
            };
            manager.UserRoles = new List<UserRole> { new UserRole { UserId = 10, RoleId = 2 } };

            var report = new User
            {
                Id = 11,
                Username = "eng.rohit",
                Email = "rohit@prm.test",
                FullName = "Rohit Verma",
                TimesheetAccessFrozen = false, // Not frozen
                ManagerId = 10,
                Designation = "Engineer",
                Department = "Dev",
                PasswordHash = "dummy"
            };

            _db.Users.AddRange(manager, report);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.UnfreezeTimesheet(11);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            var msgProp = value.GetType().GetProperty("message");
            Assert.NotNull(msgProp);
            Assert.Equal("User's timesheet access is not currently frozen.", msgProp.GetValue(value) as string);
        }
    }
}
