using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class AllocateResourceScreen : IScreen
    {
        private readonly ApiClient _api;

        public AllocateResourceScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            while (true)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.PrintHeader(AppState.Role);
                Console.WriteLine("  Allocate Resource");
                Console.WriteLine();
                Console.WriteLine("  1. AI-Assisted Search");
                Console.WriteLine("  2. Direct Allocation");
                Console.WriteLine("  3. End Allocation");
                Console.WriteLine("  4. Back");
                Console.WriteLine();

                int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 4);

                switch (choice)
                {
                    case 1: AiAssistedSearch();  break;
                    case 2: DirectAllocation();  break;
                    case 3: EndAllocation();      break;
                    case 4:
                        AppState.CurrentScreen = "manager-menu";
                        return;
                }
            }
        }

        // ─── AI-Assisted Search ────────────────────────────────────────────────

        private void AiAssistedSearch()
        {
            Console.WriteLine();
            Console.WriteLine("  ── AI-Assisted Search ─────────────────────────────────────────────────────");
            Console.WriteLine();

            string requirement = InputHelper.GetRequiredString("  Skill Requirement Description: ");

            Console.Write("  Max Hours (leave blank to skip): ");
            string? maxHoursInput = Console.ReadLine();
            int? maxHours = int.TryParse(maxHoursInput, out int parsed) ? parsed : null;

            try
            {
                var dto = new SkillMatchRequestDto
                {
                    Requirement = requirement,
                    MaxHours    = maxHours
                };

                var result = _api.PostAsync<SkillMatchRequestDto, AiResponseDto>("api/ai/skill-match", dto)
                                 .GetAwaiter().GetResult();

                Console.WriteLine();
                Console.WriteLine(result?.Result);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write("  Press Enter to continue...");
            Console.ReadLine();
        }

        // ─── Direct Allocation ─────────────────────────────────────────────────

        private void DirectAllocation()
        {
            Console.WriteLine();
            Console.WriteLine("  ── Direct Allocation ──────────────────────────────────────────────────────");
            Console.WriteLine();

            int projectId     = InputHelper.GetValidIntOption("  Project ID:        ", 1, int.MaxValue);
            int employeeId    = InputHelper.GetValidIntOption("  Employee ID:       ", 1, int.MaxValue);
            int utilization   = InputHelper.GetValidIntOption("  Utilisation % (1-100): ", 1, 100);
            DateTime startDate = InputHelper.GetValidDate("  Start Date (yyyy-MM-dd): ");
            DateTime endDate   = InputHelper.GetValidDate("  End Date   (yyyy-MM-dd): ");

            try
            {
                var result = _api.PostAsync<CreateAllocationRequest, AllocationModel>(
                    "api/allocations",
                    new CreateAllocationRequest
                    {
                        EmployeeId     = employeeId,
                        ProjectId      = projectId,
                        UtilizationPct = utilization,
                        StartDate      = startDate,
                        EndDate        = endDate
                    }
                ).GetAwaiter().GetResult();

                Console.WriteLine();
                Console.WriteLine($"  Allocation created successfully (ID: {result?.Id}).");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        // ─── End Allocation ────────────────────────────────────────────────────

        private void EndAllocation()
        {
            Console.WriteLine();
            Console.WriteLine("  ── End Allocation ─────────────────────────────────────────────────────────");
            Console.WriteLine();

            int projectId = InputHelper.GetValidIntOption("  Project ID: ", 1, int.MaxValue);

            try
            {
                var allocations = _api.GetAsync<List<AllocationModel>>($"api/allocations/project/{projectId}")
                                      .GetAwaiter().GetResult()
                                  ?? new List<AllocationModel>();

                var active = allocations.Where(a => a.IsActive).ToList();

                if (active.Count == 0)
                {
                    Console.WriteLine("  No active allocations found for this project.");
                    Console.WriteLine();
                    Console.WriteLine("  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                Console.WriteLine();
                PrintAllocationTable(active);
                Console.WriteLine();

                int allocationId = InputHelper.GetValidIntOption("  Enter Allocation ID to end: ", 1, int.MaxValue);

                _api.PutAsync($"api/allocations/{allocationId}/end")
                    .GetAwaiter().GetResult();

                Console.WriteLine();
                Console.WriteLine($"  Allocation {allocationId} ended successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        // ─── Table printer ─────────────────────────────────────────────────────

        private static void PrintAllocationTable(List<AllocationModel> allocations)
        {
            Console.WriteLine(
                $"  {"ID",-5} {"Employee ID",-12} {"Project ID",-11} {"Util %",-8} {"Start",-12} {"End",-12}");
            Console.WriteLine(
                $"  {new string('─', 5),-5} {new string('─', 12),-12} {new string('─', 11),-11} {new string('─', 8),-8} {new string('─', 12),-12} {new string('─', 12),-12}");

            foreach (var a in allocations)
            {
                Console.WriteLine(
                    $"  {a.Id,-5} {a.EmployeeId,-12} {a.ProjectId,-11} {a.UtilizationPct,-8} {a.StartDate:yyyy-MM-dd,-12} {a.EndDate:yyyy-MM-dd,-12}");
            }
        }
    }
}
