using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Employee
{
    public class ViewMyAllocationsScreen : IScreen
    {
        private readonly ApiClient _api;

        public ViewMyAllocationsScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  ╔══════════════════════════════════════════════╗");
            Console.WriteLine("  ║    MY ALLOCATIONS                            ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                var allocations = _api.GetAsync<List<AllocationModel>>(
                    $"api/allocations/employee/{AppState.UserId}"
                ).GetAwaiter().GetResult() ?? new List<AllocationModel>();

                if (allocations.Count == 0)
                {
                    Console.WriteLine("  No allocations found.");
                }
                else
                {
                    PrintAllocationsTable(allocations);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching allocations: {ex.Message}");
            }

            Pause();
            AppState.CurrentScreen = "employee-menu";
        }

        private void PrintAllocationsTable(List<AllocationModel> allocations)
        {
            Console.WriteLine($"  {"Project",-17} {"%",-6} {"From",-12} {"To",-12} {"Status"}");
            Console.WriteLine($"  {new string('─', 58)}");

            int activeUtilisation = 0;

            foreach (var a in allocations)
            {
                var p = _api.GetAsync<ProjectModel>($"api/projects/{a.ProjectId}").GetAwaiter().GetResult();
                string pName = p?.Name ?? $"Project {a.ProjectId}";
                string status = a.IsActive ? "ACTIVE" : "ENDED";
                
                if (a.IsActive)
                {
                    activeUtilisation += a.UtilizationPct;
                }

                Console.WriteLine($"  {pName,-17} {$"{a.UtilizationPct}%",-6} {a.StartDate,-12:dd-MM-yyyy} {a.EndDate,-12:dd-MM-yyyy} {status}");
            }
            
            Console.WriteLine($"  {new string('─', 58)}");
            Console.WriteLine($"  Total Utilisation: {activeUtilisation}%");
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.Write("  [B] Back > ");
            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.B || key == ConsoleKey.Enter) break;
            }
            Console.WriteLine();
        }
    }
}
