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
            await ValidateProjectHoursCapAsync(dto);
            await ValidateNoDuplicateAsync(dto);

            var timesheet = new Timesheet
            {
                UserId = dto.EmployeeId,
                ProjectId = dto.ProjectId,
                WeekStart = dto.WeekStart.Date,
                HoursLogged = dto.HoursLogged,
                Status = "SUBMITTED",
                SubmittedAt = DateTime.UtcNow
            };

            var created = await _timesheetRepository.AddAsync(timesheet);

            await AttachTagsAsync(created.Id, dto.TagIds);

            return await _timesheetRepository.GetByIdAsync(created.Id);
        }

        // --- Private helpers ---

        private async Task ValidateProjectHoursCapAsync(SubmitTimesheetDto dto)
        {
            var weekEnd = dto.WeekStart.Date.AddDays(6);

            var allocation = await _db.Allocations
                .FirstOrDefaultAsync(a =>
                    a.UserId    == dto.EmployeeId &&
                    a.ProjectId == dto.ProjectId  &&
                    a.IsActive                    &&
                    a.StartDate.Date <= weekEnd   &&
                    a.EndDate.Date   >= dto.WeekStart.Date);

            if (allocation == null)
                throw new InvalidOperationException(
                    $"No active allocation found for employee {dto.EmployeeId} " +
                    $"on project {dto.ProjectId} during week {dto.WeekStart:dd-MM-yyyy}.");

            var maxHours = float.Parse(_configuration["Timesheet:MaxWeeklyHours"] ?? "40");
            var projectCap = (allocation.UtilizationPct / 100f) * maxHours;

            if (dto.HoursLogged > projectCap)
                throw new ArgumentException(
                    $"Hours logged ({dto.HoursLogged}) exceed the project cap of {projectCap} hrs " +
                    $"({allocation.UtilizationPct}% allocation × {maxHours} hrs/week).");
        }

        private async Task ValidateNoDuplicateAsync(SubmitTimesheetDto dto)
        {
            var exists = await _db.Timesheets.AnyAsync(t =>
                t.UserId    == dto.EmployeeId &&
                t.ProjectId == dto.ProjectId  &&
                t.WeekStart.Date == dto.WeekStart.Date);

            if (exists)
                throw new InvalidOperationException(
                    $"A timesheet for project {dto.ProjectId} and week " +
                    $"{dto.WeekStart:dd-MM-yyyy} has already been submitted.");
        }

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
                    SubmittedAt = now
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
