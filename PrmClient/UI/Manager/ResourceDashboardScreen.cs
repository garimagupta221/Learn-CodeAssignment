using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class ResourceDashboardScreen : IScreen
    {
        private readonly ApiClient _api;

        public ResourceDashboardScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  Resource Dashboard");
            Console.WriteLine();

            try
            {
                var employees = _api.GetAsync<List<EmployeeModel>>($"api/employees/by-manager/{AppState.UserId}")
                                    .GetAwaiter().GetResult()
                                ?? new List<EmployeeModel>();

                var onBench = employees.Where(e => e.Status?.Equals("On Bench", StringComparison.OrdinalIgnoreCase) == true
                                                || e.Status?.Equals("OnBench", StringComparison.OrdinalIgnoreCase) == true).ToList();
                var active  = employees.Where(e => !onBench.Contains(e)).ToList();

                // ── ON BENCH ──────────────────────────────────────────────────
                Console.WriteLine("  ── ON BENCH ──────────────────────────────────────────────────────────────");
                Console.WriteLine();
                if (onBench.Count == 0)
                {
                    Console.WriteLine("  No employees currently on bench.");
                }
                else
                {
                    PrintEmployeeTable(onBench);
                }

                Console.WriteLine();

                // ── ACTIVE EMPLOYEES ──────────────────────────────────────────
                Console.WriteLine("  ── ACTIVE EMPLOYEES ──────────────────────────────────────────────────────");
                Console.WriteLine();
                if (active.Count == 0)
                {
                    Console.WriteLine("  No active employees found.");
                }
                else
                {
                    PrintEmployeeTable(active);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching employees: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("  1. Drill into employee details");
            Console.WriteLine("  2. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 2);

            if (choice == 1)
            {
                DrillEmployeeDetails();
            }

            AppState.CurrentScreen = "manager-menu";
        }

        private void DrillEmployeeDetails()
        {
            Console.WriteLine();
            int id = InputHelper.GetValidIntOption("  Enter Employee ID: ", 1, int.MaxValue);

            try
            {
                var emp = _api.GetAsync<EmployeeModel>($"api/employees/{id}")
                              .GetAwaiter().GetResult();

                if (emp == null)
                {
                    Console.WriteLine("  Employee not found.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"  {"ID:",-15} {emp.Id}");
                    Console.WriteLine($"  {"Full Name:",-15} {emp.FullName}");
                    Console.WriteLine($"  {"Email:",-15} {emp.Email}");
                    Console.WriteLine($"  {"Department:",-15} {emp.Department}");
                    Console.WriteLine($"  {"Designation:",-15} {emp.Designation}");
                    Console.WriteLine($"  {"Status:",-15} {emp.Status}");
                    Console.WriteLine($"  {"Active:",-15} {(emp.IsActive ? "Yes" : "No")}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        private static void PrintEmployeeTable(List<EmployeeModel> employees)
        {
            Console.WriteLine(
                $"  {"ID",-5} {"Full Name",-25} {"Email",-30} {"Department",-15} {"Designation",-20} {"Status",-12}");
            Console.WriteLine(
                $"  {new string('─', 5),-5} {new string('─', 25),-25} {new string('─', 30),-30} {new string('─', 15),-15} {new string('─', 20),-20} {new string('─', 12),-12}");

            foreach (var e in employees)
            {
                Console.WriteLine(
                    $"  {e.Id,-5} {e.FullName,-25} {e.Email,-30} {e.Department,-15} {e.Designation,-20} {e.Status,-12}");
            }
        }
    }
}
