using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Employee
{
    public class EmployeeMenu : IScreen
    {
        private readonly ApiClient _api;

        public EmployeeMenu(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            // ── Freeze status banner ──────────────────────────────────────────
            bool isFrozen = IsTimesheetFrozen();
            if (isFrozen)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  🔒 Your timesheet submission access is currently FROZEN.");
                Console.WriteLine("     Please contact your reporting manager to restore access.");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ──────────────────────────────────────────────");
                Console.ResetColor();
            }

            // ── Missed-timesheet reminder ──────────────────────────────────────
            DateTime previousMonday = GetPreviousMonday();
            bool isMissing = IsMissingTimesheetForWeek(previousMonday);

            if (isMissing && !isFrozen)
            {
                Console.WriteLine($"  ⚠  Reminder: Timesheet for week {previousMonday:dd-MM-yyyy} has not been submitted.");
                Console.WriteLine("  ──────────────────────────────────────────────");
            }

            Console.WriteLine("  1. Submit Timesheet");
            Console.WriteLine("  2. View My Timesheets");
            Console.WriteLine("  3. View My Allocations");
            Console.WriteLine("  4. Logout");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 4);

            AppState.CurrentScreen = choice switch
            {
                1 => "employee-submit-timesheet",
                2 => "employee-timesheets",
                3 => "employee-allocations",
                4 => "logout",
                _ => "employee-menu"
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private bool IsMissingTimesheetForWeek(DateTime weekMonday)
        {
            try
            {
                var timesheets = _api.GetAsync<List<TimesheetModel>>(
                    $"api/timesheets/employee/{AppState.UserId}"
                ).GetAwaiter().GetResult() ?? new List<TimesheetModel>();

                // A timesheet exists for that week (any status, including MISSED) means the
                // server already recorded it; we only warn when there is NO record at all.
                return !timesheets.Any(t => t.WeekStart.Date == weekMonday.Date);
            }
            catch
            {
                return false; // If the check fails, silently skip the banner
            }
        }

        private static DateTime GetPreviousMonday()
        {
            var today = DateTime.Now.Date;
            int daysToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            // Previous Monday = current Monday minus 7 days
            return today.AddDays(-daysToMonday - 7);
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
                return false; // If the check fails, silently skip the banner
            }
        }
    }
}
