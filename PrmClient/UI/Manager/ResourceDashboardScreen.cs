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

                var onBench = employees.Where(e =>
                    e.Status?.Equals("BENCH",   StringComparison.OrdinalIgnoreCase) == true
                    || e.Status?.Equals("On Bench", StringComparison.OrdinalIgnoreCase) == true
                    || e.Status?.Equals("OnBench",  StringComparison.OrdinalIgnoreCase) == true).ToList();
                var active = employees.Where(e => !onBench.Contains(e)).ToList();

                Console.WriteLine("  ── ON BENCH ──────────────────────────────────────────────────────────────");
                Console.WriteLine();
                if (onBench.Count == 0)
                {
                    Console.WriteLine("  No employees currently on bench.");
                }
                else
                {
                    PrintOnBenchTable(onBench);
                }

                Console.WriteLine();

                Console.WriteLine("  ── ACTIVE EMPLOYEES ──────────────────────────────────────────────────────");
                Console.WriteLine();
                if (active.Count == 0)
                {
                    Console.WriteLine("  No active employees found.");
                }
                else
                {
                    PrintActiveTable(active);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching employees: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write("  [D] Drill into employee details     [B] Back > ");
            string choice = Console.ReadLine()?.Trim().ToUpper() ?? "B";

            if (choice == "D")
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
                var employees = _api.GetAsync<List<EmployeeModel>>($"api/employees/by-manager/{AppState.UserId}").GetAwaiter().GetResult() ?? new List<EmployeeModel>();
                if (!employees.Any(e => e.Id == id))
                {
                    Console.WriteLine();
                    Console.WriteLine("  Employee not found or not assigned to you.");
                    Console.WriteLine("  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                var emp = _api.GetAsync<EmployeeModel>($"api/employees/{id}").GetAwaiter().GetResult();
                if (emp == null)
                {
                    Console.WriteLine("  Employee not found.");
                    Console.WriteLine("  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                var allocs       = _api.GetAsync<List<AllocationModel>>($"api/allocations/employee/{id}").GetAwaiter().GetResult() ?? new List<AllocationModel>();
                var activeAllocs = allocs.Where(a => a.IsActive && a.StartDate <= DateTime.UtcNow && a.EndDate >= DateTime.UtcNow).ToList();
                int currentUtil  = activeAllocs.Sum(a => a.UtilizationPct);
                string currentStatusStr = currentUtil == 0 ? "BENCH" : $"ALLOCATED ({currentUtil}%)";

                var skills    = _api.GetAsync<List<EmployeeSkillModel>>($"api/employees/{id}/skills").GetAwaiter().GetResult() ?? new List<EmployeeSkillModel>();
                string skillsStr = skills.Count > 0 ? string.Join(", ", skills.Select(s => s.SkillName)) : "None";

                Console.WriteLine();
                Console.WriteLine($"  ── {emp.FullName} ─────────────────────────────────");
                Console.WriteLine($"  Department     : {emp.Department}");
                Console.WriteLine($"  Current Status : {currentStatusStr}");
                Console.WriteLine($"  Profile Skills : {skillsStr}");
                Console.WriteLine();

                Console.WriteLine("  Active Allocations:");
                if (activeAllocs.Count == 0)
                {
                    Console.WriteLine("    None");
                }
                else
                {
                    Console.WriteLine($"    {"Project",-16} {"%",-5} {"From",-12} {"To"}");
                    foreach (var a in activeAllocs)
                    {
                        var p      = _api.GetAsync<ProjectModel>($"api/projects/{a.ProjectId}").GetAwaiter().GetResult();
                        string pName = p?.Name ?? a.ProjectId.ToString();
                        Console.WriteLine($"    {pName,-16} {$"{a.UtilizationPct}%",-5} {a.StartDate,-12:dd-MM-yyyy} {a.EndDate:dd-MM-yyyy}");
                    }
                }
                Console.WriteLine();

                var timesheets = _api.GetAsync<List<TimesheetModel>>($"api/timesheets/employee/{id}").GetAwaiter().GetResult() ?? new List<TimesheetModel>();
                var recentT    = timesheets.Where(t => t.WeekStart >= DateTime.UtcNow.AddDays(-28)).ToList();
                var tags       = recentT.SelectMany(t => t.TimesheetTags ?? new List<TimesheetTagModel>()).Select(t => t.ActivityTag?.TagName).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();

                Console.WriteLine("  Recent Activity Tags (last 4 weeks):");
                if (tags.Count == 0)
                {
                    Console.WriteLine("    None");
                }
                else
                {
                    Console.WriteLine($"    {string.Join(", ", tags)}");
                }
                Console.WriteLine();

                Console.Write("  [B] Back > ");
                while (true)
                {
                    string back = Console.ReadLine()?.Trim().ToUpper() ?? "";
                    if (back == "B") break;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                Console.WriteLine("  Press any key to continue...");
                Console.ReadKey(intercept: true);
            }
        }

        private void PrintOnBenchTable(List<EmployeeModel> employees)
        {
            Console.WriteLine($"  {"ID",-5} {"Name",-16} {"Department",-15} {"Skills"}");
            Console.WriteLine($"  {new string('─', 5),-5} {new string('─', 16),-16} {new string('─', 15),-15} {new string('─', 30)}");
            foreach (var e in employees)
            {
                string skillsStr = "";
                try
                {
                    var skills = _api.GetAsync<List<EmployeeSkillModel>>($"api/employees/{e.Id}/skills").GetAwaiter().GetResult();
                    if (skills != null && skills.Count > 0)
                        skillsStr = string.Join(", ", skills.Select(s => s.SkillName));
                }
                catch { }

                Console.WriteLine($"  {e.Id,-5} {e.FullName,-16} {e.Department,-15} {skillsStr}");
            }
        }

        private void PrintActiveTable(List<EmployeeModel> employees)
        {
            Console.WriteLine($"  {"ID",-5} {"Name",-16} {"Alloc %",-9} {"Availability"}");
            Console.WriteLine($"  {new string('─', 5),-5} {new string('─', 16),-16} {new string('─', 9),-9} {new string('─', 15)}");
            foreach (var e in employees)
            {
                int allocPct = 0;
                try
                {
                    var allocs = _api.GetAsync<List<AllocationModel>>($"api/allocations/employee/{e.Id}").GetAwaiter().GetResult();
                    if (allocs != null)
                    {
                        allocPct = allocs.Where(a => a.IsActive && a.StartDate <= DateTime.UtcNow && a.EndDate >= DateTime.UtcNow).Sum(a => a.UtilizationPct);
                    }
                }
                catch { }

                string availability = allocPct >= 100 ? "FULL" : $"{100 - allocPct}% free";
                Console.WriteLine($"  {e.Id,-5} {e.FullName,-16} {$"{allocPct}%",-9} {availability}");
            }
        }
    }
}
