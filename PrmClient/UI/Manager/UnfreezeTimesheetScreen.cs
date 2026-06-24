using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    /// <summary>
    /// Shows the manager a list of their direct reports whose timesheet access is frozen.
    /// For each frozen employee the manager chooses:
    ///   1 – Revoke  (restore access)
    ///   2 – Do Not Revoke (keep frozen, no action taken)
    /// </summary>
    public class UnfreezeTimesheetScreen : IScreen
    {
        private readonly ApiClient _api;

        public UnfreezeTimesheetScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  🔒 Frozen Timesheets – Manage Access");
            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.WriteLine();

            List<FrozenEmployeeModel> frozen;
            try
            {
                frozen = _api.GetAsync<List<FrozenEmployeeModel>>("api/users/frozen-timesheets")
                    .GetAwaiter().GetResult() ?? new List<FrozenEmployeeModel>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Failed to load frozen employees: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  Press any key to return...");
                Console.ReadKey(true);
                AppState.CurrentScreen = "manager-menu";
                return;
            }

            if (!frozen.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✅ No employees currently have their timesheet access frozen.");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  Press any key to return...");
                Console.ReadKey(true);
                AppState.CurrentScreen = "manager-menu";
                return;
            }

            Console.WriteLine($"  {frozen.Count} employee(s) with frozen timesheet access:");
            Console.WriteLine();

            foreach (var emp in frozen)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  👤 {emp.FullName}  ({emp.Email})");
                Console.ResetColor();
                Console.WriteLine("     Options:");
                Console.WriteLine("       1. Revoke (restore timesheet access)");
                Console.WriteLine("       2. Do Not Revoke (keep frozen)");
                Console.WriteLine();

                int choice = InputHelper.GetValidIntOption("     Select option: ", 1, 2);

                if (choice == 1)
                {
                    try
                    {
                        _api.PostAsync($"api/users/{emp.UserId}/unfreeze-timesheet")
                            .GetAwaiter().GetResult();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"     ✅ {emp.FullName}'s timesheet access has been restored.");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"     ✗ Failed to restore access: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"     ↩  No action taken for {emp.FullName}.");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.WriteLine("  Done. Press any key to return to the menu...");
            Console.ReadKey(true);
            AppState.CurrentScreen = "manager-menu";
        }
    }
}
