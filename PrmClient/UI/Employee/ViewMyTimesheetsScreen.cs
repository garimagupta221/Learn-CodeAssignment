using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Employee
{
    public class ViewMyTimesheetsScreen : IScreen
    {
        private readonly ApiClient _api;

        public ViewMyTimesheetsScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            List<TimesheetModel> timesheets;
            try
            {
                timesheets = _api.GetAsync<List<TimesheetModel>>(
                    $"api/timesheets/employee/{AppState.UserId}"
                ).GetAwaiter().GetResult() ?? new List<TimesheetModel>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching timesheets: {ex.Message}");
                Pause();
                AppState.CurrentScreen = "employee-menu";
                return;
            }

            // Group by week, summing hours across projects per week
            var weekSummaries = timesheets
                .GroupBy(t => t.WeekStart.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => (
                    WeekStart: g.Key,
                    TotalHrs: g.Sum(t => t.HoursLogged),
                    // Surface MISSED if any entry for that week is MISSED, else first status
                    Status: g.Any(t => t.Status == "MISSED") ? "MISSED" : g.First().Status
                ))
                .ToList();

            Console.WriteLine($"  {"Week Start",-16} {"Total Hrs",-12} Status");
            Console.WriteLine($"  {new string('─', 44)}");

            if (weekSummaries.Count == 0)
            {
                Console.WriteLine("  No timesheets found.");
            }
            else
            {
                foreach (var (weekStart, totalHrs, status) in weekSummaries)
                {
                    string statusDisplay = status == "MISSED" ? "MISSED    ⚠" : status;
                    Console.WriteLine($"  {weekStart:dd-MMM-yyyy,-16} {totalHrs + " hrs",-12} {statusDisplay}");
                }
            }

            Console.WriteLine($"  {new string('─', 44)}");
            Console.WriteLine();
            Console.Write("  [V] View week details     [B] Back → ");
            string action = Console.ReadLine()?.Trim().ToUpper() ?? "B";

            if (action == "V" && weekSummaries.Count > 0)
            {
                ViewWeekDetail(timesheets);
            }

            Pause();
            AppState.CurrentScreen = "employee-menu";
        }

        // ─── Week detail ───────────────────────────────────────────────────────

        private void ViewWeekDetail(List<TimesheetModel> allTimesheets)
        {
            Console.WriteLine();
            Console.Write("  Enter week start (DD-MM-YYYY): ");
            string? input = Console.ReadLine()?.Trim();

            if (!DateTime.TryParseExact(input, "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var weekDate))
            {
                Console.WriteLine("  Invalid date.");
                return;
            }

            var weekSheets = allTimesheets
                .Where(t => t.WeekStart.Date == weekDate.Date)
                .ToList();

            if (weekSheets.Count == 0)
            {
                Console.WriteLine("  No records found for that week.");
                return;
            }

            string weekStatus = weekSheets.Any(t => t.Status == "MISSED") ? "MISSED" : weekSheets.First().Status;
            Console.WriteLine();
            Console.WriteLine($"  ── Week: {weekDate:dd-MMM-yyyy} — Status: {weekStatus} ─────");
            Console.WriteLine();
            Console.WriteLine($"  {"Project",-22} {"Hrs",-7} Status");
            Console.WriteLine($"  {new string('─', 44)}");

            foreach (var t in weekSheets)
            {
                string statusDisplay = t.Status == "MISSED" ? "MISSED ⚠" : t.Status;
                Console.WriteLine($"  {"Project " + t.ProjectId,-22} {t.HoursLogged,-7:F0} {statusDisplay}");
            }

            Console.WriteLine($"  {new string('─', 44)}");
            Console.WriteLine($"  Total: {weekSheets.Sum(t => t.HoursLogged):F0} hrs");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("  Press any key to return to menu...");
            Console.ReadKey(intercept: true);
        }
    }
}
