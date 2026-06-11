using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class ViewAllocationsScreen : IScreen
    {
        private readonly ApiClient _api;

        public ViewAllocationsScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("View Allocations");
            Console.WriteLine();

            try
            {
                var allocations = _api.GetAsync<List<AllocationModel>>("api/allocations")
                                      .GetAwaiter().GetResult()
                                  ?? new List<AllocationModel>();

                if (allocations.Count == 0)
                    Console.WriteLine("  No allocations found.");
                else
                    PrintAllocationTable(allocations);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("  1. Back");
            Console.WriteLine();
            InputHelper.GetValidIntOption("Enter option: ", 1, 1);
            AppState.CurrentScreen = "admin-menu";
        }

        // ─── Table printer ────────────────────────────────────────────────────

        private static void PrintAllocationTable(List<AllocationModel> allocations)
        {
            Console.WriteLine(
                $"  {"ID",-5} {"Employee",-10} {"Project",-10} {"%",-5} {"From",-12} {"To",-12} {"Active",-6}");
            Console.WriteLine(
                $"  {new string('-', 5),-5} {new string('-', 10),-10} {new string('-', 10),-10} {new string('-', 5),-5} {new string('-', 12),-12} {new string('-', 12),-12} {new string('-', 6),-6}");

            foreach (var a in allocations)
            {
                Console.WriteLine(
                    $"  {a.Id,-5} {a.EmployeeId,-10} {a.ProjectId,-10} {a.UtilizationPct,-5} {a.StartDate:yyyy-MM-dd,-12} {a.EndDate:yyyy-MM-dd,-12} {(a.IsActive ? "Yes" : "No"),-6}");
            }
        }
    }
}

