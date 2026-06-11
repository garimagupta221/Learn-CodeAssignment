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
            Console.WriteLine("  My Allocations");
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

        private static void PrintAllocationsTable(List<AllocationModel> allocations)
        {
            Console.WriteLine(
                $"  {"ID",-5} {"Project ID",-11} {"Utilisation %",-15} {"Start Date",-12} {"End Date",-12} {"Active",-7}");
            Console.WriteLine(
                $"  {new string('─', 5),-5} {new string('─', 11),-11} {new string('─', 15),-15} {new string('─', 12),-12} {new string('─', 12),-12} {new string('─', 7),-7}");

            foreach (var a in allocations)
            {
                Console.WriteLine(
                    $"  {a.Id,-5} {a.ProjectId,-11} {a.UtilizationPct,-15} {a.StartDate:yyyy-MM-dd,-12} {a.EndDate:yyyy-MM-dd,-12} {(a.IsActive ? "Yes" : "No"),-7}");
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
