using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class ManagerTimesheetsScreen : IScreen
    {
        private readonly ApiClient _api;

        public ManagerTimesheetsScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            // Week selection — Enter = current week
            DateTime defaultMonday = GetCurrentWeekMonday();
            Console.Write("  Filter by week (DD-MM-YYYY) or press Enter for current week: ");
            string? weekInput = Console.ReadLine()?.Trim();

            DateTime weekStart;
            if (string.IsNullOrEmpty(weekInput))
            {
                weekStart = defaultMonday;
            }
            else if (DateTime.TryParseExact(weekInput, "dd-MM-yyyy",
                         System.Globalization.CultureInfo.InvariantCulture,
                         System.Globalization.DateTimeStyles.None, out var parsed))
            {
                weekStart = parsed.Date;
            }
            else
            {
                Console.WriteLine("  Invalid date. Use DD-MM-YYYY.");
                Pause();
                AppState.CurrentScreen = "manager-menu";
                return;
            }

            Console.WriteLine($"  Week: {weekStart:dd-MMM-yyyy}");
            Console.WriteLine();

            // (empName, projName, hours, status)
            var rows = new List<(string empName, string projName, float hours, string status)>();

            try
            {
                var employees = _api.GetAsync<List<EmployeeModel>>(
                        $"api/employees/by-manager/{AppState.UserId}")
                    .GetAwaiter().GetResult() ?? new List<EmployeeModel>();

                var projects = _api.GetAsync<List<ProjectModel>>("api/projects")
                    .GetAwaiter().GetResult() ?? new List<ProjectModel>();

                var projNames = projects.ToDictionary(p => p.Id, p => p.Name);

                foreach (var emp in employees)
                {
                    var sheets = _api.GetAsync<List<TimesheetModel>>(
                            $"api/timesheets/employee/{emp.Id}")
                        .GetAwaiter().GetResult() ?? new List<TimesheetModel>();

                    foreach (var t in sheets.Where(t => t.WeekStart.Date == weekStart.Date))
                    {
                        string pName = projNames.TryGetValue(t.ProjectId, out var n) ? n : $"Project {t.ProjectId}";
                        rows.Add((emp.FullName, pName, t.HoursLogged, t.Status));
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                Pause();
                AppState.CurrentScreen = "manager-menu";
                return;
            }

            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.WriteLine($"  {"Employee",-18} {"Project",-18} {"Hrs",-7} Status");
            Console.WriteLine("  ──────────────────────────────────────────────");

            if (rows.Count == 0)
            {
                Console.WriteLine("  No timesheets found for this week.");
            }
            else
            {
                foreach (var (empName, projName, hours, status) in rows)
                {
                    string statusDisplay = status == "MISSED" ? "MISSED ⚠" : status;
                    Console.WriteLine($"  {empName,-18} {projName,-18} {hours,-7:F0} {statusDisplay}");
                }
            }

            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.WriteLine();
            Console.Write("  [V] View employee timesheet detail     [B] Back → ");
            string action = Console.ReadLine()?.Trim().ToUpper() ?? "B";

            if (action == "V" && rows.Count > 0)
            {
                ViewEmployeeDetail(rows, weekStart);
            }

            Pause();
            AppState.CurrentScreen = "manager-menu";
        }

        // ─── Detail view ───────────────────────────────────────────────────────

        private static void ViewEmployeeDetail(
            List<(string empName, string projName, float hours, string status)> rows,
            DateTime weekStart)
        {
            Console.WriteLine();
            Console.Write("  Enter employee name (or part): ");
            string filter = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;

            var matched = rows
                .Where(r => r.empName.ToLower().Contains(filter))
                .ToList();

            if (matched.Count == 0)
            {
                Console.WriteLine("  No matching employee found.");
                return;
            }

            string empName = matched[0].empName;
            Console.WriteLine();
            Console.WriteLine($"  ── {empName} — Week {weekStart:dd-MMM-yyyy} ──────────────────────");
            Console.WriteLine($"  {"Project",-22} {"Hrs",-7} Status");
            Console.WriteLine($"  {new string('─', 40)}");

            foreach (var r in matched)
            {
                string statusDisplay = r.status == "MISSED" ? "MISSED ⚠" : r.status;
                Console.WriteLine($"  {r.projName,-22} {r.hours,-7:F0} {statusDisplay}");
            }

            Console.WriteLine($"  {new string('─', 40)}");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static DateTime GetCurrentWeekMonday()
        {
            var today = DateTime.Now.Date;
            int daysToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return today.AddDays(-daysToMonday);
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("  Press any key to return to menu...");
            Console.ReadKey(intercept: true);
        }
    }
}
