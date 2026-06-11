using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrmServer.Entities;
using PrmServer.Providers;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class AiService : IAiService
    {
        private readonly PrmDbContext _db;
        private readonly ISystemConfigService _config;
        private readonly IEnumerable<IAiProvider> _providers;

        public AiService(PrmDbContext db, ISystemConfigService config, IEnumerable<IAiProvider> providers)
        {
            _db = db;
            _config = config;
            _providers = providers;
        }

        private IAiProvider GetActiveProvider()
        {
            var providerName = _config.Get("ActiveAiProvider") ?? "Gemini";
            return _providers.FirstOrDefault(p => p.ProviderName == providerName)
                ?? throw new InvalidOperationException($"AI provider '{providerName}' is not registered.");
        }

        private string GetApiKey()
        {
            var key = _config.Get("AiApiKey");
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("AiApiKey is not configured in System Configuration.");
            return key;
        }

        public async Task<string> GetSkillMatchAsync(string requirement, int projectId, int? maxHours, int managerUserId)
        {
            var employees = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Include(u => u.Timesheets)
                    .ThenInclude(t => t.TimesheetTags)
                        .ThenInclude(tt => tt.ActivityTag)
                .Where(u => u.IsActive && u.ManagerId == managerUserId && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            var employeeLines = employees.Select(e =>
            {
                var skills = e.UserSkills
                    .Select(us => $"{us.Skill.Name} ({us.Proficiency})")
                    .ToList();

                var recentTags = e.Timesheets
                    .OrderByDescending(t => t.WeekStart)
                    .Take(5)
                    .SelectMany(t => t.TimesheetTags.Select(tt => tt.ActivityTag.TagName))
                    .Distinct()
                    .ToList();

                var totalHours = e.Timesheets
                    .Where(t => t.ProjectId == projectId)
                    .Sum(t => t.HoursLogged);

                var parts = new List<string>
                {
                    $"- {e.FullName} | {e.Designation} | {e.Department}",
                    $"  Skills: {(skills.Any() ? string.Join(", ", skills) : "None")}",
                    $"  Recent Activity Tags: {(recentTags.Any() ? string.Join(", ", recentTags) : "None")}",
                    $"  Hours on Project {projectId}: {totalHours}"
                };

                if (maxHours.HasValue)
                    parts.Add($"  Max Available Hours: {maxHours.Value}");

                return string.Join("\n", parts);
            });

            var prompt =
                $"You are a resource planning assistant. Based on the following employee profiles, " +
                $"identify who best matches this requirement: \"{requirement}\".\n\n" +
                $"Employees:\n{string.Join("\n\n", employeeLines)}\n\n" +
                $"Provide a ranked list with brief justification for each recommendation.";

            return await GetActiveProvider().GenerateContentAsync(prompt, GetApiKey());
        }

        public async Task<string> GetProjectRiskSummaryAsync(int projectId)
        {
            var project = await _db.Projects
                .Include(p => p.Milestones)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            var today = DateTime.UtcNow;

            var milestoneLines = project.Milestones.Select(m =>
                $"- {m.Title} | Due: {m.DueDate:yyyy-MM-dd} | Status: {m.Status}" +
                (m.CompletedAt.HasValue ? $" | Completed: {m.CompletedAt:yyyy-MM-dd}" : string.Empty));

            var allocationLines = project.Allocations
                .Where(a => a.IsActive)
                .Select(a =>
                    $"- {a.User.FullName} | {a.UtilizationPct}% | " +
                    $"{a.StartDate:yyyy-MM-dd} to {a.EndDate:yyyy-MM-dd}");

            var overdueMilestones = project.Milestones
                .Count(m => m.Status != "Completed" && m.DueDate < today);

            var prompt =
                $"You are a project risk analyst. Analyze the following project data and provide a concise risk summary.\n\n" +
                $"Project: {project.Name}\n" +
                $"Status: {project.Status} | Health: {project.Health}\n" +
                $"Timeline: {project.StartDate:yyyy-MM-dd} to {project.EndDate:yyyy-MM-dd}\n" +
                $"Overdue Milestones: {overdueMilestones}\n\n" +
                $"Milestones:\n{string.Join("\n", milestoneLines)}\n\n" +
                $"Active Allocations:\n{string.Join("\n", allocationLines)}\n\n" +
                $"Identify key risks (timeline, resource, scope) and suggest mitigations.";

            return await GetActiveProvider().GenerateContentAsync(prompt, GetApiKey());
        }
    }
}
