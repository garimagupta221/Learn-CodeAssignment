using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Employee
{
    public class SubmitTimesheetScreen : IScreen
    {
        private readonly ApiClient _api;

        private static readonly string[] ActivityTagNames =
        {
            "Backend API Development",
            "Microservices / Architecture",
            "Database Design & Queries",
            "WebSocket / Real-time Features",
            "Frontend Development",
            "Code Review / Mentoring",
            "Bug Fixing",
            "DevOps / Deployment",
            "Testing & QA",
            "Documentation",
            "Other"
        };

        public SubmitTimesheetScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  SUBMIT TIMESHEET");
            Console.WriteLine();

            if (IsTimesheetFrozen())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  🔒 Your timesheet access is currently FROZEN.");
                Console.WriteLine("     You cannot create, update, or submit timesheet entries.");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  Please contact your reporting manager to restore access.");
                Pause();
                AppState.CurrentScreen = "employee-menu";
                return;
            }

            Console.WriteLine($"  Employee  : {AppState.FullName}");

            DateTime weekStart = PromptWeekStart();
            Console.WriteLine();

            if (weekStart.Date > DateTime.Today)
            {
                Console.WriteLine("  Cannot submit timesheets for future weeks.");
                Pause();
                AppState.CurrentScreen = "employee-menu";
                return;
            }

            try
            {
                Console.WriteLine("  Checking your active allocations for this week...");
                Console.WriteLine();

                var allocations = _api.GetAsync<List<AllocationModel>>(
                    $"api/allocations/employee/{AppState.UserId}"
                ).GetAwaiter().GetResult() ?? new List<AllocationModel>();

                var activeAllocations = allocations
                    .Where(a => a.IsActive
                        && a.StartDate.Date <= weekStart.Date
                        && a.EndDate.Date   >= weekStart.Date)
                    .ToList();

                if (activeAllocations.Count == 0)
                {
                    Console.WriteLine("  You have no active allocations for that week.");
                    Pause();
                    AppState.CurrentScreen = "employee-menu";
                    return;
                }

                var timesheets = _api.GetAsync<List<TimesheetModel>>(
                    $"api/timesheets/employee/{AppState.UserId}"
                ).GetAwaiter().GetResult() ?? new List<TimesheetModel>();

                var submittedProjectIds = timesheets
                    .Where(t => t.WeekStart.Date == weekStart.Date)
                    .Select(t => t.ProjectId)
                    .ToHashSet();

                var pendingAllocations = activeAllocations
                    .Where(a => !submittedProjectIds.Contains(a.ProjectId))
                    .ToList();

                if (pendingAllocations.Count == 0)
                {
                    Console.WriteLine("  You have already submitted timesheets for all active allocations for this week.");
                    Pause();
                    AppState.CurrentScreen = "employee-menu";
                    return;
                }

                var serverTags    = FetchActivityTags();
                var entries       = new List<(AllocationModel Allocation, float Hours, List<int> TagIds, List<string> TagNames)>();
                int maxWeeklyHours = GetMaxWeeklyHours();

                for (int i = 0; i < pendingAllocations.Count; i++)
                {
                    var allocation   = pendingAllocations[i];
                    string projectName  = allocation.ProjectName ?? $"Project {allocation.ProjectId}";
                    int expectedMaxHours = (int)Math.Round(allocation.UtilizationPct / 100.0 * maxWeeklyHours);

                    Console.WriteLine($"  ──────────────────────────────────────────────");
                    Console.WriteLine($"  PROJECT {i + 1} OF {pendingAllocations.Count} — {projectName}");
                    Console.WriteLine($"    Allocation: {allocation.UtilizationPct}%   |   Expected: {expectedMaxHours} hrs max");
                    Console.WriteLine($"  ──────────────────────────────────────────────");

                    float hours = GetValidHours("  Hours worked this week: ", expectedMaxHours);

                    Console.WriteLine();
                    Console.WriteLine("  What did you work on? Select activity tags:");
                    Console.WriteLine();
                    PrintTagMenu(serverTags);

                    Console.Write("  Select tags (comma-separated): ");
                    string? tagInput    = Console.ReadLine()?.Trim();
                    var (tagIds, tagNames) = ParseTagSelections(tagInput, serverTags);

                    entries.Add((allocation, hours, tagIds, tagNames));
                    Console.WriteLine();
                }

                PrintSummary(entries, maxWeeklyHours);

                Console.Write("  [S] Submit Timesheet     [B] Back\n  > ");
                string? choice = Console.ReadLine()?.Trim().ToUpperInvariant();

                if (choice != "S")
                {
                    AppState.CurrentScreen = "employee-menu";
                    return;
                }

                int submitted = 0;
                foreach (var (allocation, hours, tagIds, _) in entries)
                {
                    try
                    {
                        _api.PostAsync<SubmitTimesheetRequest, TimesheetModel>(
                            "api/timesheets",
                            new SubmitTimesheetRequest
                            {
                                EmployeeId  = AppState.UserId,
                                ProjectId   = allocation.ProjectId,
                                WeekStart   = weekStart,
                                HoursLogged = hours,
                                TagIds      = tagIds
                            }
                        ).GetAwaiter().GetResult();

                        submitted++;
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"  ✗ Error submitting for {allocation.ProjectName ?? $"Project {allocation.ProjectId}"}: {ex.Message}");
                    }
                }

                Console.WriteLine();
                if (submitted == entries.Count)
                    Console.WriteLine("  Timesheet submitted successfully. Status: SUBMITTED ✓");
                else
                    Console.WriteLine($"  {submitted} of {entries.Count} timesheet(s) submitted. Check errors above.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching allocations: {ex.Message}");
            }

            Pause();
            AppState.CurrentScreen = "employee-menu";
        }

        private DateTime PromptWeekStart()
        {
            Console.Write("  Week Start: Enter date (DD-MM-YYYY) or press Enter for last Monday\n  > ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                var today = DateTime.Today;
                int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                var lastMonday = today.AddDays(-daysSinceMonday);
                Console.WriteLine($"  Using: {lastMonday:dd-MM-yyyy}");
                return lastMonday;
            }

            if (DateTime.TryParseExact(input, "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsed))
            {
                if (parsed.DayOfWeek != DayOfWeek.Monday)
                {
                    Console.WriteLine("  Week start must be a Monday. Using last Monday.");
                    int daysSinceMonday = ((int)parsed.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    return parsed.AddDays(-daysSinceMonday);
                }
                return parsed;
            }

            Console.WriteLine("  Invalid date. Using last Monday.");
            var fallback = DateTime.Today;
            int days = ((int)fallback.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return fallback.AddDays(-days);
        }

        private List<ActivityTagModel> FetchActivityTags()
        {
            try
            {
                var tags = _api.GetAsync<List<ActivityTagModel>>("api/activity-tags")
                    .GetAwaiter().GetResult();
                if (tags != null && tags.Count > 0)
                    return tags;
            }
            catch { /* fall through to built-in list */ }

            return ActivityTagNames.Select((name, idx) => new ActivityTagModel
            {
                Id      = idx + 1,
                TagName = name
            }).ToList();
        }

        private static void PrintTagMenu(List<ActivityTagModel> tags)
        {
            for (int i = 0; i < tags.Count; i++)
                Console.WriteLine($"  {i + 1,3}.  {tags[i].TagName}");
            Console.WriteLine();
        }

        private static (List<int> tagIds, List<string> tagNames) ParseTagSelections(
            string? input, List<ActivityTagModel> tags)
        {
            var tagIds   = new List<int>();
            var tagNames = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return (tagIds, tagNames);

            foreach (var part in input.Split(','))
            {
                if (!int.TryParse(part.Trim(), out int idx) || idx < 1 || idx > tags.Count)
                    continue;

                var tag = tags[idx - 1];
                tagIds.Add(tag.Id);
                tagNames.Add(tag.TagName);
            }

            return (tagIds, tagNames);
        }

        private static float GetValidHours(string prompt, int projectMaxHours)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!float.TryParse(input, out float value) || value <= 0)
                {
                    Console.WriteLine("  Invalid input. Please enter a number greater than 0.");
                    continue;
                }

                if (value > projectMaxHours)
                {
                    Console.WriteLine($"  Hours cannot exceed the project cap of {projectMaxHours} hrs " +
                                      $"(your allocation % × max weekly hours). Please try again.");
                    continue;
                }

                return value;
            }
        }

        private static int GetMaxWeeklyHours() => 40;

        private static void PrintSummary(
            List<(AllocationModel Allocation, float Hours, List<int> TagIds, List<string> TagNames)> entries,
            int maxWeeklyHours)
        {
            float totalHours = entries.Sum(e => e.Hours);
            string totalCheck = totalHours <= maxWeeklyHours ? "✓" : "⚠ exceeds max";

            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.WriteLine("  SUMMARY");
            foreach (var (allocation, hours, _, tagNames) in entries)
            {
                string projectName = allocation.ProjectName ?? $"Project {allocation.ProjectId}";
                string tags = tagNames.Count > 0 ? $"[{string.Join(", ", tagNames)}]" : "[no tags]";
                Console.WriteLine($"    {projectName,-20} {hours,4} hrs    {tags}");
            }
            Console.WriteLine($"    {"─────────────────────────────────────────"}");
            Console.WriteLine($"    Total           {totalHours,4} hrs / {maxWeeklyHours} hrs max   {totalCheck}");
            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.WriteLine();
        }

        private bool IsTimesheetFrozen()
        {
            try
            {
                var result = _api.GetAsync<FreezeStatusModel>(
                    $"api/users/{AppState.UserId}/freeze-status"
                ).GetAwaiter().GetResult();
                return result?.IsTimesheetFrozen ?? false;
            }
            catch
            {
                return false;
            }
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("  Press any key to return to menu...");
            Console.ReadKey(intercept: true);
        }
    }
}
