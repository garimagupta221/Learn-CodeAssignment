using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly ITimesheetRepository _timesheetRepository;
        private readonly ITimesheetTagRepository _timesheetTagRepository;
        private readonly IConfiguration _configuration;
        private readonly PrmDbContext _db;

        public TimesheetService(
            ITimesheetRepository timesheetRepository,
            ITimesheetTagRepository timesheetTagRepository,
            IConfiguration configuration,
            PrmDbContext db)
        {
            _timesheetRepository = timesheetRepository;
            _timesheetTagRepository = timesheetTagRepository;
            _configuration = configuration;
            _db = db;
        }

        public Task<List<Timesheet>> GetByEmployeeAsync(int employeeId)
        {
            return _timesheetRepository.GetByUserIdAsync(employeeId);
        }

        public Task<Timesheet?> GetByIdAsync(int id)
        {
            return _timesheetRepository.GetByIdAsync(id);
        }

        public async Task<Timesheet> SubmitAsync(SubmitTimesheetDto dto)
        {
            ValidateWeekStart(dto.WeekStart);
            ValidateHours(dto.HoursLogged);

            var timesheet = new Timesheet
            {
                UserId = dto.EmployeeId,
                ProjectId = dto.ProjectId,
                WeekStart = dto.WeekStart.Date,
                HoursLogged = dto.HoursLogged,
                Status = "PENDING",
                RejectionReason = string.Empty,
                SubmittedAt = DateTime.UtcNow
            };

            var created = await _timesheetRepository.AddAsync(timesheet);

            await AttachTagsAsync(created.Id, dto.TagIds);

            // Fetch created timesheet eagerly loading relationships
            return await _timesheetRepository.GetByIdAsync(created.Id);
        }

        public async Task<Timesheet> UpdateAsync(int id, UpdateTimesheetDto dto)
        {
            var timesheet = await _timesheetRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Timesheet {id} not found.");

            if (timesheet.Status != "REJECTED")
                throw new InvalidOperationException("Only rejected timesheets can be resubmitted.");

            ValidateHours(dto.HoursLogged);

            timesheet.HoursLogged = dto.HoursLogged;
            timesheet.Status = "PENDING";
            timesheet.RejectionReason = string.Empty;
            timesheet.SubmittedAt = DateTime.UtcNow;

            var updated = await _timesheetRepository.UpdateAsync(timesheet);

            await ReplaceTagsAsync(id, dto.TagIds);

            return await _timesheetRepository.GetByIdAsync(id);
        }

        public async Task ApproveAsync(int id, int approverId)
        {
            var timesheet = await _timesheetRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Timesheet {id} not found.");

            if (timesheet.Status != "PENDING")
                throw new InvalidOperationException("Only pending timesheets can be approved.");

            timesheet.Status = "APPROVED";
            timesheet.ApprovedBy = approverId;
            timesheet.ReviewedAt = DateTime.UtcNow;

            await _timesheetRepository.UpdateAsync(timesheet);
        }

        public async Task RejectAsync(int id, string reason, int approverId)
        {
            var timesheet = await _timesheetRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Timesheet {id} not found.");

            if (timesheet.Status != "PENDING")
                throw new InvalidOperationException("Only pending timesheets can be rejected.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A rejection reason must be provided.");

            timesheet.Status = "REJECTED";
            timesheet.RejectionReason = reason;
            timesheet.ApprovedBy = approverId;
            timesheet.ReviewedAt = DateTime.UtcNow;

            await _timesheetRepository.UpdateAsync(timesheet);
        }

        // --- Private helpers ---

        private void ValidateWeekStart(DateTime weekStart)
        {
            if (weekStart.Date > DateTime.UtcNow.Date)
                throw new ArgumentException("Week start date cannot be in the future.");

            if (weekStart.DayOfWeek != DayOfWeek.Monday)
                throw new ArgumentException("Week start must be a Monday.");
        }

        private void ValidateHours(float hours)
        {
            var maxHours = float.Parse(
                _configuration["Timesheet:MaxWeeklyHours"] ?? "60");

            if (hours <= 0)
                throw new ArgumentException("Hours logged must be greater than zero.");

            if (hours > maxHours)
                throw new ArgumentException(
                    $"Hours logged cannot exceed the weekly maximum of {maxHours} hours.");
        }

        private async Task AttachTagsAsync(int timesheetId, List<int> tagIds)
        {
            foreach (var tagId in tagIds)
            {
                await _timesheetTagRepository.AddAsync(new TimesheetTag
                {
                    TimesheetId = timesheetId,
                    ActivityTagId = tagId
                });
            }
        }

        private async Task ReplaceTagsAsync(int timesheetId, List<int> tagIds)
        {
            var existing = await _timesheetTagRepository.GetAllAsync();
            var toDelete = existing.Where(t => t.TimesheetId == timesheetId).ToList();

            foreach (var tag in toDelete)
                await _timesheetTagRepository.DeleteAsync(tag.Id);

            await AttachTagsAsync(timesheetId, tagIds);
        }

        public async Task<List<Employee>> GetEmployeesMissingCurrentWeekAsync()
        {
            // Current week starts on the most recent Monday (UTC)
            var today = DateTime.UtcNow.Date;
            int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var weekStart = today.AddDays(-daysSinceMonday);

            // All active employees (users with Engineer role) who have at least one active allocation
            var activeUserIds = await _db.Allocations
                .Where(a => a.IsActive)
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            // Employees who already submitted a timesheet for this week
            var submittedUserIds = await _db.Timesheets
                .Where(t => t.WeekStart.Date == weekStart)
                .Select(t => t.UserId)
                .Distinct()
                .ToListAsync();

            var missingIds = activeUserIds.Except(submittedUserIds).ToList();

            var missingUsers = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .Where(u => u.IsActive && missingIds.Contains(u.Id) && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            return missingUsers.Select(u => new Employee
            {
                Id = u.Id,
                UserId = u.Id,
                ManagerUserId = u.ManagerId,
                FullName = u.FullName,
                Email = u.Email,
                Department = u.Department ?? string.Empty,
                Designation = u.Designation ?? string.Empty,
                Status = u.Status?.Status ?? "BENCH",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                User = u
            }).ToList();
        }

        public async Task MarkMissedTimesheetsAsync()
        {
            // Previous week's Monday
            var today = DateTime.UtcNow.Date;
            int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var previousMonday = today.AddDays(-daysSinceMonday - 7);
            var previousSunday = previousMonday.AddDays(6);

            // Active employees who had an active allocation during the previous week
            var allocatedUserIds = await _db.Allocations
                .Where(a => a.IsActive
                    && a.StartDate.Date <= previousSunday
                    && a.EndDate.Date >= previousMonday)
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            // Employees who already have any timesheet for the previous week's start date
            var existingTimesheetUserIds = await _db.Timesheets
                .Where(t => t.WeekStart.Date == previousMonday)
                .Select(t => t.UserId)
                .Distinct()
                .ToListAsync();

            var missingUserIds = allocatedUserIds
                .Except(existingTimesheetUserIds)
                .ToList();

            if (!missingUserIds.Any())
                return;

            // Resolve a project for each missing employee (first active allocation's project)
            var allocations = await _db.Allocations
                .Where(a => a.IsActive
                    && missingUserIds.Contains(a.UserId)
                    && a.StartDate.Date <= previousSunday
                    && a.EndDate.Date >= previousMonday)
                .ToListAsync();

            var employeeProjectMap = allocations
                .GroupBy(a => a.UserId)
                .ToDictionary(g => g.Key, g => g.First().ProjectId);

            var now = DateTime.UtcNow;
            foreach (var employeeId in missingUserIds)
            {
                if (!employeeProjectMap.TryGetValue(employeeId, out var projectId))
                    continue;

                _db.Timesheets.Add(new Timesheet
                {
                    UserId = employeeId,
                    ProjectId = projectId,
                    WeekStart = previousMonday,
                    HoursLogged = 0,
                    Status = "MISSED",
                    RejectionReason = string.Empty,
                    SubmittedAt = now
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
