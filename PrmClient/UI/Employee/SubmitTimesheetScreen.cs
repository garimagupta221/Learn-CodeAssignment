using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Employee
{
    public class SubmitTimesheetScreen : IScreen
    {
        private readonly ApiClient _api;

        public SubmitTimesheetScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  Submit Timesheet");
            Console.WriteLine();

            DateTime weekStart = InputHelper.GetValidDate("  Week Start Date (dd-MM-yyyy): ");
            Console.WriteLine();

            try
            {
                // Fetch active allocations for this employee to determine eligible projects
                var allocations = _api.GetAsync<List<AllocationModel>>(
                    $"api/allocations/employee/{AppState.UserId}"
                ).GetAwaiter().GetResult() ?? new List<AllocationModel>();

                var activeAllocations = allocations
                    .Where(a => a.IsActive && a.StartDate.Date <= weekStart.Date && a.EndDate.Date >= weekStart.Date)
                    .ToList();

                if (activeAllocations.Count == 0)
                {
                    Console.WriteLine("  You have no active allocations for that week.");
                    Pause();
                    AppState.CurrentScreen = "employee-menu";
                    return;
                }

                Console.WriteLine($"  Active projects for week of {weekStart:dd-MM-yyyy}:");
                Console.WriteLine();

                int submitted = 0;

                foreach (var allocation in activeAllocations)
                {
                    Console.WriteLine($"  ── Project ID: {allocation.ProjectId} (Utilisation: {allocation.UtilizationPct}%) ──");

                    float hours = GetValidHours("  Hours Worked: ");

                    Console.Write("  Activity Tag IDs (comma-separated, or leave blank): ");
                    string? tagInput = Console.ReadLine()?.Trim();
                    var tagIds = ParseTagIds(tagInput);

                    try
                    {
                        var result = _api.PostAsync<SubmitTimesheetRequest, TimesheetModel>(
                            "api/timesheets",
                            new SubmitTimesheetRequest
                            {
                                EmployeeId  = AppState.UserId,
                                ProjectId   = allocation.ProjectId,
                                WeekStart   = weekStart,
                                HoursLogged = hours,
                                TagIds      = tagIds
                            }
                        ).GetAwaiter().GetResult();

                        Console.WriteLine($"  ✓ Timesheet submitted (ID: {result?.Id}).");
                        submitted++;
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"  Error submitting for project {allocation.ProjectId}: {ex.Message}");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"  {submitted} timesheet(s) submitted successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching allocations: {ex.Message}");
            }

            Pause();
            AppState.CurrentScreen = "employee-menu";
        }

        private static float GetValidHours(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (float.TryParse(input, out float value) && value > 0 && value <= 168)
                    return value;

                Console.WriteLine("  Invalid hours. Please enter a number between 0.1 and 168.");
            }
        }

        private static List<int> ParseTagIds(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<int>();

            var ids = new List<int>();
            foreach (var part in input.Split(','))
            {
                if (int.TryParse(part.Trim(), out int id))
                    ids.Add(id);
            }
            return ids;
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("  Press any key to return to menu...");
            Console.ReadKey(intercept: true);
        }
    }
}
