using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.BackgroundTasks;
using PrmServer.Services.Interfaces;
using Xunit;

namespace PrmServer.Tests.Services
{
    public class TimesheetReminderTaskTests : IDisposable
    {
        private readonly PrmDbContext _db;
        private readonly Mock<ITimesheetReminderRepository> _reminderRepoMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly TimesheetReminderTask _sut;

        private static int _counter = 0;

        public TimesheetReminderTaskTests()
        {
            var options = new DbContextOptionsBuilder<PrmDbContext>()
                .UseInMemoryDatabase($"TimesheetReminderDb_{System.Threading.Interlocked.Increment(ref _counter)}")
                .Options;

            _db = new PrmDbContext(options);
            _reminderRepoMock = new Mock<ITimesheetReminderRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _emailServiceMock = new Mock<IEmailService>();

            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(PrmDbContext))).Returns(_db);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITimesheetReminderRepository))).Returns(_reminderRepoMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationService))).Returns(_notificationServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmailService))).Returns(_emailServiceMock.Object);

            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            _sut = new TimesheetReminderTask(NullLogger<TimesheetReminderTask>.Instance);
        }

        public void Dispose() => _db.Dispose();

        private static DateTime GetPreviousMonday()
        {
            var today = DateTime.UtcNow.Date;
            int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return today.AddDays(-daysSinceMonday - 7);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendReminder1_WhenNoReminderLogExists()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "eng.test",
                Email = "test@prm.com",
                FullName = "Test Employee",
                TimesheetAccessFrozen = false,
                Department = "Dev",
                Designation = "SE",
                PasswordHash = "dummy_hash"
            };
            _db.Users.Add(user);

            var previousMonday = GetPreviousMonday();
            _db.Timesheets.Add(new Timesheet
            {
                UserId = user.Id,
                ProjectId = 1,
                WeekStart = previousMonday,
                HoursLogged = 0,
                Status = "MISSED",
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            _reminderRepoMock.Setup(r => r.GetByUserAndWeekAsync(user.Id, previousMonday))
                .ReturnsAsync((TimesheetReminderLog?)null);

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            _reminderRepoMock.Verify(r => r.AddAsync(It.Is<TimesheetReminderLog>(l => 
                l.UserId == user.Id && 
                l.WeekStart == previousMonday && 
                l.Reminder1SentAt != null && 
                l.Reminder2SentAt == null && 
                !l.IsFrozen)), Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync(
                user.Email, user.FullName,
                It.Is<string>(s => s.Contains("Timesheet Reminder")),
                It.IsAny<string>()), Times.Once);

            _notificationServiceMock.Verify(n => n.CreateAsync(
                user.Id,
                It.Is<string>(s => s.Contains("Reminder:")),
                "TIMESHEET_REMINDER_1"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendReminder2_WhenReminder1ExistsAndIsOver8HoursAgo()
        {
            // Arrange
            var user = new User
            {
                Id = 2,
                Username = "eng.test2",
                Email = "test2@prm.com",
                FullName = "Test Employee 2",
                TimesheetAccessFrozen = false,
                Department = "Dev",
                Designation = "SE",
                PasswordHash = "dummy_hash"
            };
            _db.Users.Add(user);

            var previousMonday = GetPreviousMonday();
            _db.Timesheets.Add(new Timesheet
            {
                UserId = user.Id,
                ProjectId = 1,
                WeekStart = previousMonday,
                HoursLogged = 0,
                Status = "MISSED",
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var existingLog = new TimesheetReminderLog
            {
                UserId = user.Id,
                WeekStart = previousMonday,
                Reminder1SentAt = DateTime.UtcNow.AddHours(-9) // 9 hours ago (> 8 working hours)
            };

            _reminderRepoMock.Setup(r => r.GetByUserAndWeekAsync(user.Id, previousMonday))
                .ReturnsAsync(existingLog);

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            _reminderRepoMock.Verify(r => r.UpdateAsync(It.Is<TimesheetReminderLog>(l => 
                l.Reminder2SentAt != null && 
                !l.IsFrozen)), Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync(
                user.Email, user.FullName,
                It.Is<string>(s => s.Contains("Final Timesheet Reminder")),
                It.IsAny<string>()), Times.Once);

            _notificationServiceMock.Verify(n => n.CreateAsync(
                user.Id,
                It.Is<string>(s => s.Contains("Final Reminder:")),
                "TIMESHEET_REMINDER_2"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFreezeAccess_WhenReminder2ExistsAndIsOver8HoursAgo()
        {
            // Arrange
            var user = new User
            {
                Id = 3,
                Username = "eng.test3",
                Email = "test3@prm.com",
                FullName = "Test Employee 3",
                TimesheetAccessFrozen = false,
                ManagerId = 4,
                Department = "Dev",
                Designation = "SE",
                PasswordHash = "dummy_hash"
            };
            var manager = new User
            {
                Id = 4,
                Username = "mgr.test4",
                Email = "mgr4@prm.com",
                FullName = "Test Manager 4",
                Department = "Dev",
                Designation = "Manager",
                PasswordHash = "dummy_hash"
            };
            _db.Users.AddRange(user, manager);

            var previousMonday = GetPreviousMonday();
            _db.Timesheets.Add(new Timesheet
            {
                UserId = user.Id,
                ProjectId = 1,
                WeekStart = previousMonday,
                HoursLogged = 0,
                Status = "MISSED",
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var existingLog = new TimesheetReminderLog
            {
                UserId = user.Id,
                WeekStart = previousMonday,
                Reminder1SentAt = DateTime.UtcNow.AddHours(-18),
                Reminder2SentAt = DateTime.UtcNow.AddHours(-9) // 9 hours ago (> 8 working hours)
            };

            _reminderRepoMock.Setup(r => r.GetByUserAndWeekAsync(user.Id, previousMonday))
                .ReturnsAsync(existingLog);

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            Assert.True(user.TimesheetAccessFrozen); // User should be frozen in DB

            _reminderRepoMock.Verify(r => r.UpdateAsync(It.Is<TimesheetReminderLog>(l => 
                l.IsFrozen == true)), Times.Once);

            // Verify email to employee
            _emailServiceMock.Verify(e => e.SendAsync(
                user.Email, user.FullName,
                It.Is<string>(s => s.Contains("Timesheet Access Frozen")),
                It.IsAny<string>()), Times.Once);

            // Verify email to manager
            _emailServiceMock.Verify(e => e.SendAsync(
                manager.Email, manager.FullName,
                It.Is<string>(s => s.Contains("Timesheet Access Has Been Frozen")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFreezeAccessAndSkipManagerEmail_WhenReminder2ExistsAndNoManagerIsAssigned()
        {
            // Arrange
            var user = new User
            {
                Id = 5,
                Username = "eng.test5",
                Email = "test5@prm.com",
                FullName = "Test Employee 5",
                TimesheetAccessFrozen = false,
                ManagerId = null,
                Department = "Dev",
                Designation = "SE",
                PasswordHash = "dummy_hash"
            };
            _db.Users.Add(user);

            var previousMonday = GetPreviousMonday();
            _db.Timesheets.Add(new Timesheet
            {
                UserId = user.Id,
                ProjectId = 1,
                WeekStart = previousMonday,
                HoursLogged = 0,
                Status = "MISSED",
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var existingLog = new TimesheetReminderLog
            {
                UserId = user.Id,
                WeekStart = previousMonday,
                Reminder1SentAt = DateTime.UtcNow.AddHours(-18),
                Reminder2SentAt = DateTime.UtcNow.AddHours(-9)
            };

            _reminderRepoMock.Setup(r => r.GetByUserAndWeekAsync(user.Id, previousMonday))
                .ReturnsAsync(existingLog);

            // Act
            await _sut.ExecuteAsync(_serviceScopeMock.Object, CancellationToken.None);

            // Assert
            Assert.True(user.TimesheetAccessFrozen);

            _reminderRepoMock.Verify(r => r.UpdateAsync(It.Is<TimesheetReminderLog>(l => 
                l.IsFrozen == true)), Times.Once);

            // Verify email to employee
            _emailServiceMock.Verify(e => e.SendAsync(
                user.Email, user.FullName,
                It.Is<string>(s => s.Contains("Timesheet Access Frozen")),
                It.IsAny<string>()), Times.Once);

            // Verify no email to manager
            _emailServiceMock.Verify(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Timesheet Access Has Been Frozen")),
                It.IsAny<string>()), Times.Never);
        }
    }
}
