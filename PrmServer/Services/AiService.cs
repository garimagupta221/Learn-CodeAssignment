using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Providers;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    /// <summary>
    /// Orchestrates AI-powered features (Skill Match, Risk Summary).
    /// Uses the Factory / Strategy pattern: the active provider is resolved at runtime
    /// from the admin-configurable "ActiveAiProvider" system config key, allowing the
    /// administrator to switch between Gemma, Gemini, and Grok without a restart.
    /// </summary>
    public class AiService : IAiService
    {
        private readonly PrmDbContext _db;
        private readonly ISystemConfigService _config;
        private readonly IEnumerable<IAiProvider> _providers;
        private readonly ILogger<AiService> _logger;

        public AiService(
            PrmDbContext db,
            ISystemConfigService config,
            IEnumerable<IAiProvider> providers,
            ILogger<AiService> logger)
        {
            _db = db;
            _config = config;
            _providers = providers;
            _logger = logger;
        }

        // ── Factory lookup ──────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the currently configured AI provider from the DI container.
        /// Throws <see cref="InvalidOperationException"/> if the configured provider
        /// name is not registered (open extension point — add a new IAiProvider class
        /// + DI registration and it becomes available without any other changes).
        /// </summary>
        private IAiProvider GetActiveProvider()
        {
            var providerName = _config.Get("ActiveAiProvider");
            if (string.IsNullOrWhiteSpace(providerName))
                providerName = "Gemma"; // default to Gemma

            return _providers.FirstOrDefault(p => p.ProviderName == providerName)
                ?? throw new InvalidOperationException(
                    $"AI provider '{providerName}' is not registered. " +
                    $"Available providers: {string.Join(", ", _providers.Select(p => p.ProviderName))}");
        }

        private string GetApiKey()
        {
            var key = _config.Get("AiApiKey");
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException(
                    "AiApiKey is not configured in System Configuration.");
            return key;
        }

        // ── Skill Match ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Gathers employee profiles (skills, recent activity tags, project hours)
        /// for engineers under <paramref name="managerUserId"/> and asks the active
        /// LLM to rank them against the natural-language <paramref name="requirement"/>.
        /// </summary>
        public async Task<SkillMatchResult> GetSkillMatchAsync(
            string requirement, int? projectId, int? maxHours, int managerUserId)
        {
            var caller = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == managerUserId)
                ?? throw new KeyNotFoundException($"User {managerUserId} not found.");

            bool isCallerAdmin = caller.UserRoles.Any(ur => ur.Role.RoleName == "Admin");
            int? targetManagerId = null;

            if (projectId.HasValue)
            {
                var project = await _db.Projects.FindAsync(projectId.Value)
                    ?? throw new KeyNotFoundException($"Project {projectId.Value} not found.");

                if (!isCallerAdmin && project.ManagerId != managerUserId)
                {
                    throw new UnauthorizedAccessException("Managers can only perform skill match on projects they manage.");
                }
                targetManagerId = project.ManagerId;
            }
            else
            {
                if (!isCallerAdmin)
                {
                    targetManagerId = managerUserId;
                }
            }

            var query = _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Include(u => u.Timesheets)
                    .ThenInclude(t => t.TimesheetTags)
                        .ThenInclude(tt => tt.ActivityTag)
                .Where(u => u.IsActive
                    && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"));

            if (targetManagerId.HasValue)
            {
                query = query.Where(u => u.ManagerId == targetManagerId.Value);
            }

            var employees = await query.ToListAsync();

            if (!employees.Any())
                throw new InvalidOperationException(
                    targetManagerId.HasValue
                        ? $"No active engineers found under manager {targetManagerId.Value}."
                        : "No active engineers found.");

            var activeAllocations = await _db.Allocations
                .Where(a => a.IsActive && a.StartDate <= DateTime.UtcNow && a.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            // Gather only the required context fields, serialized as JSON
            var employeeContextList = employees.Select(e =>
            {
                var hasAllocation = activeAllocations.Any(a => a.UserId == e.Id);
                var status = hasAllocation ? "Allocated" : "Bench";

                var recentTags = e.Timesheets
                    .OrderByDescending(t => t.WeekStart)
                    .Take(5)
                    .SelectMany(t => e.Timesheets.Where(ts => ts.Id == t.Id).SelectMany(ts => ts.TimesheetTags.Select(tt => tt.ActivityTag.TagName)))
                    .Distinct()
                    .ToList();

                return new
                {
                    EmployeeId = e.Id,
                    FullName = e.FullName,
                    Status = status,
                    Skills = e.UserSkills.Select(us => new
                    {
                        SkillName = us.Skill.Name,
                        Proficiency = us.Proficiency
                    }).ToList(),
                    RecentActivityTags = recentTags
                };
            }).ToList();

            var employeesJson = System.Text.Json.JsonSerializer.Serialize(employeeContextList);

            var prompt =
                $"You are a resource planning assistant. Analyze the following employee profiles against this requirement: \"{requirement}\".\n\n" +
                $"Employees Context (JSON):\n{employeesJson}\n\n" +
                $"IMPORTANT INSTRUCTIONS:\n" +
                $"1. ONLY include employees whose skills are relevant to the requirement. If an employee has NO matching or related skills, EXCLUDE them entirely.\n" +
                $"2. Rank the matching employees from best fit to least fit.\n" +
                $"3. Ranking criteria (in order of priority): (a) number of directly matching skills, (b) proficiency level (Expert/Advanced > Intermediate > Beginner), (c) availability (Bench/free > partially allocated > fully allocated).\n" +
                $"4. Be smart about related skills — e.g. if the requirement mentions 'ML', also consider Python, TensorFlow, Scikit-learn etc. as relevant.\n" +
                $"5. The response must be a JSON array with no markdown code blocks and no extra text outside the JSON array.\n" +
                $"6. If NO employees match the requirement at all, return an empty array: []\n\n" +
                $"Each object in the array must have these keys:\n" +
                $"- \"EmployeeId\" (integer, must match the ID from context)\n" +
                $"- \"FullName\" (string)\n" +
                $"- \"SkillsMatch\" (string, comma-separated list of matching skills)\n" +
                $"- \"Availability\" (string, e.g. \"100% free\", \"50% free\", \"Fully Allocated\")\n" +
                $"- \"RecentActivity\" (string, brief summary of recent activity tags or \"None\")\n" +
                $"- \"Reason\" (string, explain why this person is ranked here — mention matching skills and proficiency)\n\n" +
                $"Format as: [ {{ \"EmployeeId\": 123, \"FullName\": \"...\", \"SkillsMatch\": \"...\", \"Availability\": \"...\", \"RecentActivity\": \"...\", \"Reason\": \"...\" }} ]";

            var provider = GetActiveProvider();
            var responseText = await provider.GenerateContentAsync(prompt, GetApiKey());

            var cleanedJson = CleanJsonText(responseText);
            List<SkillMatchRecommendationDto> recommendations;
            try
            {
                recommendations = System.Text.Json.JsonSerializer.Deserialize<List<SkillMatchRecommendationDto>>(cleanedJson, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<SkillMatchRecommendationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize LLM skill match response. Raw response was: {Raw}", responseText);
                throw new InvalidOperationException("AI response was not in the expected structured format. Please try again.");
            }

            return new SkillMatchResult(recommendations, provider.ProviderName);
        }

        private static string CleanJsonText(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "[]";

            json = json.Trim();
            if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                json = json.Substring(7);
            }
            else if (json.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                json = json.Substring(3);
            }

            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }

            return json.Trim();
        }

        // ── Risk Summary ────────────────────────────────────────────────────────────

        /// <summary>
        /// Collects milestone and timesheet data for the project and asks the active LLM
        /// for a plain-English risk summary.  If the LLM call fails for any reason
        /// (network issue, provider down, config missing) a graceful fallback summary is
        /// returned so the UI never receives a 500 error for this informational feature.
        /// </summary>
        public async Task<AiResult> GetProjectRiskSummaryAsync(int projectId, int managerUserId)
        {
            var project = await _db.Projects
                .Include(p => p.Milestones)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            var caller = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == managerUserId)
                ?? throw new KeyNotFoundException($"User {managerUserId} not found.");

            bool isCallerAdmin = caller.UserRoles.Any(ur => ur.Role.RoleName == "Admin");

            if (!isCallerAdmin && project.ManagerId != managerUserId)
            {
                throw new UnauthorizedAccessException("You cannot view the status of project not allocated to you");
            }

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

            try
            {
                var provider = GetActiveProvider();
                var result = await provider.GenerateContentAsync(prompt, GetApiKey());
                return new AiResult(result, provider.ProviderName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "AI Risk Summary failed for project {ProjectId}; returning fallback.", projectId);

                // Graceful fallback — derive a basic risk summary from the raw data
                var fallback = BuildFallbackRiskSummary(project, overdueMilestones);
                return new AiResult(fallback, "Fallback");
            }
        }

        // ── Team Builder ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Staffs an entire project team in a single AI call.
        /// Only 100%-bench employees are considered (no active allocation today).
        /// Deduplication and gap reasoning run server-side for determinism.
        /// Managers see all engineers company-wide.
        /// </summary>
        public async Task<TeamBuilderResult> BuildTeamAsync(TeamBuilderRequestDto request, int managerUserId)
        {
            if (string.IsNullOrWhiteSpace(request.TeamRequirement))
                throw new InvalidOperationException("Please enter your team requirements before running Team Builder.");

            var today = DateTime.UtcNow;

            // ── 1. Load active allocations ───────────────────────
            var activeAllocations = await _db.Allocations
                .Include(a => a.User)
                .Where(a => a.IsActive && a.StartDate <= today && a.EndDate >= today)
                .ToListAsync();

            var allocationMap = activeAllocations
                .GroupBy(a => a.UserId)
                .ToDictionary(g => g.Key, g => g.Max(a => a.EndDate));

            // ── 2. Load ALL engineers (active) company-wide ─────────────────────────
            var allEngineers = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .Where(u => u.IsActive && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            // ── 3. Serialize all engineers with their skills and bench/allocation status ──
            var employeesContext = allEngineers.Select(e => new
            {
                EmployeeId = e.Id,
                FullName   = e.FullName,
                Skills     = e.UserSkills.Select(us => new
                {
                    SkillName   = us.Skill.Name,
                    Proficiency = us.Proficiency
                }).ToList(),
                BenchStatus = allocationMap.TryGetValue(e.Id, out var endDate) 
                    ? $"ALLOCATED until {endDate:yyyy-MM-dd}" 
                    : "BENCH"
            }).ToList();

            var employeesJson = System.Text.Json.JsonSerializer.Serialize(employeesContext);

            // ── 4. Build AI prompt ───────────────────────────────────────────────────
            var prompt =
                $"You are a resource planning assistant. Staff a project team based on a plain English request.\n\n" +
                $"Project: \"{request.ProjectName}\"\n" +
                $"Team Request: \"{request.TeamRequirement}\"\n\n" +
                $"EMPLOYEES DATABASE (Active engineers company-wide):\n{employeesJson}\n\n" +
                $"INSTRUCTIONS:\n" +
                $"1. Parse the 'Team Request' to identify all required roles. If a quantity is specified (e.g. '2 java developers'), create separate role slots (e.g. 'Java Developer 1' and 'Java Developer 2').\n" +
                $"2. For each role slot, search the EMPLOYEES DATABASE to fill it:\n" +
                $"   - Prefer active engineers who are currently 'BENCH'.\n" +
                $"   - Ensure they have the required skills at or above a reasonable proficiency (Beginner, Intermediate, Advanced, Expert).\n" +
                $"   - Deduplicate: Do not assign the same employee to multiple roles.\n" +
                $"3. If a role can be filled:\n" +
                $"   - Set \"Filled\": true\n" +
                $"   - Set \"EmployeeId\", \"EmployeeName\", \"MatchedSkills\" (comma-separated list of their matching skills), and \"Reason\" (brief description of why they were picked).\n" +
                $"4. If a role cannot be filled by a bench candidate:\n" +
                $"   - Set \"Filled\": false\n" +
                $"   - Determine \"GapReason\" and \"GapDetail\":\n" +
                $"     - If no one in the company has the required skill, set \"GapReason\": \"NoSkill\" and \"GapDetail\": \"No employee in the company has the skill [SkillName]. Recommend hiring or training.\"\n" +
                $"     - If employees have the skill but all are allocated, set \"GapReason\": \"Allocated\" and \"GapDetail\": \"[EmployeeName] has the skill but is allocated until [EndDate].\"\n" +
                $"     - If the best match was already assigned to another role in this team and no other bench candidate is available, set \"GapReason\": \"NoAvailableBench\" and \"GapDetail\": \"The best match was already assigned to another role in this team.\"\n" +
                $"5. Return ONLY a JSON array of roles with no markdown formatting. No backticks, no other text outside the JSON array.\n" +
                $"Format for each role object:\n" +
                $"{{\n" +
                $"  \"RoleTitle\": \"...\",\n" +
                $"  \"Filled\": true/false,\n" +
                $"  \"EmployeeId\": <int or null>,\n" +
                $"  \"EmployeeName\": \"...\",\n" +
                $"  \"MatchedSkills\": \"...\",\n" +
                $"  \"Reason\": \"...\",\n" +
                $"  \"GapReason\": \"...\",\n" +
                $"  \"GapDetail\": \"...\"\n" +
                $"}}";

            var provider     = GetActiveProvider();
            var responseText = await provider.GenerateContentAsync(prompt, GetApiKey());
            var cleanedJson  = CleanJsonText(responseText);

            // ── 5. Parse AI response ─────────────────────────────────────────────────
            List<AiTeamSlot> aiSlots;
            try
            {
                aiSlots = System.Text.Json.JsonSerializer.Deserialize<List<AiTeamSlot>>(cleanedJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<AiTeamSlot>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Team Builder AI response could not be deserialized. Raw: {Raw}", responseText);
                throw new InvalidOperationException("AI response was not in the expected structured format. Please try again.");
            }

            // ── 6. Server-side deduplication ─────────────────────────────────────────
            var usedEmployeeIds = new HashSet<int>();
            var results         = new List<TeamRoleResultDto>();

            foreach (var slot in aiSlots)
            {
                bool isDuplicate = slot.EmployeeId.HasValue && usedEmployeeIds.Contains(slot.EmployeeId.Value);

                if (slot.Filled && slot.EmployeeId.HasValue && !isDuplicate)
                {
                    usedEmployeeIds.Add(slot.EmployeeId.Value);
                    results.Add(new TeamRoleResultDto
                    {
                        RoleTitle     = slot.RoleTitle ?? "Unknown Role",
                        Filled        = true,
                        EmployeeId    = slot.EmployeeId,
                        EmployeeName  = slot.EmployeeName,
                        MatchedSkills = slot.MatchedSkills,
                        Reason        = slot.Reason
                    });
                }
                else
                {
                    var gapReason = slot.GapReason;
                    var gapDetail = slot.GapDetail;

                    if (isDuplicate)
                    {
                        gapReason = "NoAvailableBench";
                        gapDetail = $"The best match for '{slot.RoleTitle}' was already assigned to another role in this team.";
                    }
                    else if (string.IsNullOrWhiteSpace(gapReason))
                    {
                        gapReason = "NoSkill";
                        gapDetail = "Could not find a suitable bench match.";
                    }

                    results.Add(new TeamRoleResultDto
                    {
                        RoleTitle = slot.RoleTitle ?? "Unknown Role",
                        Filled    = false,
                        GapReason = gapReason,
                        GapDetail = gapDetail
                    });
                }
            }

            return new TeamBuilderResult(request.ProjectName, results, provider.ProviderName);
        }

        // ─── Internal DTO for deserializing the AI team slot response ───────────────

        private class AiTeamSlot
        {
            public string RoleTitle { get; set; }
            public bool Filled { get; set; }
            public int? EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public string MatchedSkills { get; set; }
            public string Reason { get; set; }
            public string GapReason { get; set; }
            public string GapDetail { get; set; }
        }

        // ── Private helpers ─────────────────────────────────────────────────────────

        private static string BuildFallbackRiskSummary(Project project, int overdueMilestones)
        {
            var lines = new List<string>
            {
                $"⚠️ AI provider is currently unavailable. This is a system-generated summary.",
                string.Empty,
                $"Project: {project.Name}",
                $"Status: {project.Status} | Health: {project.Health}",
                $"Timeline: {project.StartDate:yyyy-MM-dd} → {project.EndDate:yyyy-MM-dd}",
                string.Empty,
            };

            if (overdueMilestones > 0)
                lines.Add($"🔴 RISK — {overdueMilestones} overdue milestone(s) detected. " +
                          "Immediate attention required to avoid further schedule slippage.");
            else
                lines.Add("✅ No overdue milestones detected.");

            var activeAllocations = project.Allocations.Count(a => a.IsActive);
            if (activeAllocations == 0)
                lines.Add("🔴 RISK — No active resource allocations on this project.");
            else
                lines.Add($"👥 {activeAllocations} active allocation(s) on record.");

            lines.Add(string.Empty);
            lines.Add("Please configure or restore the AI provider for a detailed analysis.");

            return string.Join("\n", lines);
        }
    }
}
