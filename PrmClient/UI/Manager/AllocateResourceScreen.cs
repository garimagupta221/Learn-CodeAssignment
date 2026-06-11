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
                Console.WriteLine("  1. Find resource using AI (recommended)");
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

        // ─── Find resource using AI (recommended) ────────────────────────────────────────────────

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

            try
            {
                var projects = _api.GetAsync<List<ProjectModel>>($"api/projects/manager/{AppState.UserId}").GetAwaiter().GetResult() ?? new List<ProjectModel>();
                if (projects.Count == 0)
                {
                    Console.WriteLine("  No projects assigned to you.");
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }
                
                Console.WriteLine("  Your Projects:");
                foreach (var p in projects)
                {
                    Console.WriteLine($"    {p.Id} - {p.Name}");
                }
                Console.WriteLine();

                int projectId;
                ProjectModel? selectedProject;
                while (true)
                {
                    projectId = InputHelper.GetValidIntOption("  Select Project ID: ", 1, int.MaxValue);
                    selectedProject = projects.FirstOrDefault(p => p.Id == projectId);
                    if (selectedProject == null)
                    {
                        Console.WriteLine("  Invalid Project ID. Please select from the list above.");
                    }
                    else if (!selectedProject.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && 
                             !selectedProject.Status.Equals("PLANNED", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"  Project '{selectedProject.Name}' is currently {selectedProject.Status}. Only ACTIVE or PLANNED projects are allowed.");
                    }
                    else
                    {
                        break;
                    }
                }

                var employees = _api.GetAsync<List<EmployeeModel>>($"api/employees/by-manager/{AppState.UserId}").GetAwaiter().GetResult() ?? new List<EmployeeModel>();
                if (employees.Count == 0)
                {
                    Console.WriteLine("  No employees assigned to you.");
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("  Your Team:");
                foreach (var e in employees)
                {
                    Console.WriteLine($"    {e.Id} - {e.FullName} ({e.Status})");
                }
                Console.WriteLine();

                int employeeId;
                EmployeeModel? selectedEmployee;
                while (true)
                {
                    employeeId = InputHelper.GetValidIntOption("  Enter Employee ID: ", 1, int.MaxValue);
                    selectedEmployee = employees.FirstOrDefault(e => e.Id == employeeId);
                    if (selectedEmployee != null) break;
                    Console.WriteLine("  Invalid Employee ID. Please select from the list above.");
                }

                Console.WriteLine();
                Console.WriteLine($"  ── {selectedEmployee.FullName} ─────────────────────────────────");
                
                var allocs = _api.GetAsync<List<AllocationModel>>($"api/allocations/employee/{employeeId}").GetAwaiter().GetResult() ?? new List<AllocationModel>();
                int currentUtil = allocs.Where(a => a.IsActive && a.StartDate <= DateTime.UtcNow && a.EndDate >= DateTime.UtcNow).Sum(a => a.UtilizationPct);
                
                string benchStatus = currentUtil == 0 ? "fully on bench" : (currentUtil >= 100 ? "fully allocated" : $"{100 - currentUtil}% free");
                Console.WriteLine($"  Current Utilisation: {currentUtil}%   ({benchStatus})");
                Console.WriteLine();

                Console.WriteLine("  Set Allocation:");
                int utilization = InputHelper.GetValidIntOption("    Utilisation %   : ", 1, 100);
                DateTime startDate = InputHelper.GetValidDate("    From Date (dd-MM-yyyy) : ");
                DateTime endDate = InputHelper.GetValidDate("    To Date   (dd-MM-yyyy) : ");
                
                if (startDate >= endDate)
                {
                    Console.WriteLine();
                    Console.WriteLine("    X Invalid (From Date must be before To Date)");
                    Console.WriteLine();
                    Console.WriteLine("  Press any key to go back...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("  Validating...");
                
                int overlappingUtil = allocs.Where(a => a.IsActive && a.StartDate < endDate && a.EndDate > startDate).Sum(a => a.UtilizationPct);
                int totalUtil = overlappingUtil + utilization;
                
                if (totalUtil > 100)
                {
                    Console.WriteLine($"    {selectedEmployee.FullName} total in this period: {overlappingUtil}% + {utilization}% = {totalUtil}%   X Invalid (Exceeds 100%)");
                    Console.WriteLine();
                    Console.WriteLine("  Press any key to go back...");
                    Console.ReadKey(intercept: true);
                    return;
                }
                else
                {
                    Console.WriteLine($"    {selectedEmployee.FullName} total in this period: {overlappingUtil}% + {utilization}% = {totalUtil}%   ✓ Valid");
                }

                Console.WriteLine();
                Console.Write("  [C] Confirm     [B] Back > ");
                string confirm = Console.ReadLine()?.Trim().ToUpper() ?? "B";
                
                if (confirm == "C")
                {
                    var result = _api.PostAsync<CreateAllocationRequest, AllocationModel>(
                        "api/allocations",
                        new CreateAllocationRequest
                        {
                            EmployeeId     = employeeId,
                            ProjectId      = projectId,
                            AllocatedBy    = AppState.UserId,
                            UtilizationPct = utilization,
                            StartDate      = startDate,
                            EndDate        = endDate
                        }
                    ).GetAwaiter().GetResult();

                    Console.WriteLine();
                    Console.WriteLine($"  Allocation created successfully (ID: {result?.Id}).");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("  Allocation cancelled.");
                }
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

            try
            {
                var projects = _api.GetAsync<List<ProjectModel>>($"api/projects/manager/{AppState.UserId}").GetAwaiter().GetResult() ?? new List<ProjectModel>();
                if (projects.Count == 0)
                {
                    Console.WriteLine("  No projects assigned to you.");
                    Console.WriteLine();
                    Console.WriteLine("  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                Console.WriteLine("  Your Projects:");
                foreach (var p in projects)
                {
                    Console.WriteLine($"    {p.Id} - {p.Name}");
                }
                Console.WriteLine();

                int projectId;
                ProjectModel? selectedProject;
                while (true)
                {
                    projectId = InputHelper.GetValidIntOption("  Select Project ID: ", 1, int.MaxValue);
                    selectedProject = projects.FirstOrDefault(p => p.Id == projectId);
                    if (selectedProject != null) break;
                    Console.WriteLine("  Invalid Project ID. Please select from the list above.");
                }
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
                Console.WriteLine($"  Active Allocations on this project:");
                Console.WriteLine($"    {"#",-3} {"Employee",-15} {"%",-5} {"From",-12} {"To"}");

                var activeWithNames = new List<(int Index, AllocationModel Alloc, string EmpName)>();
                int index = 1;
                foreach (var a in active)
                {
                    var emp = _api.GetAsync<EmployeeModel>($"api/employees/{a.EmployeeId}").GetAwaiter().GetResult();
                    string empName = emp?.FullName ?? $"Employee {a.EmployeeId}";
                    activeWithNames.Add((index, a, empName));
                    Console.WriteLine($"    {index + ".",-3} {empName,-15} {$"{a.UtilizationPct}%",-5} {a.StartDate,-12:dd-MM-yyyy} {a.EndDate:dd-MM-yyyy}");
                    index++;
                }
                Console.WriteLine("  ──────────────────────────────────────────────");
                Console.WriteLine();

                int selectedIndex = InputHelper.GetValidIntOption("  Select allocation to end: ", 1, active.Count);
                var selectedTuple = activeWithNames.First(t => t.Index == selectedIndex);
                var allocToEnd = selectedTuple.Alloc;
                string selectedEmpName = selectedTuple.EmpName;

                Console.WriteLine();
                Console.WriteLine($"  End {selectedEmpName}'s allocation on {selectedProject?.Name ?? "this project"}?");
                string todayStr = DateTime.UtcNow.ToString("dd-MM-yyyy");
                Console.WriteLine($"  Set end date to today ({todayStr})?");
                Console.WriteLine();
                Console.Write("  [Y] Yes, End Now    [B] Back > ");
                
                string confirm = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (confirm != "Y")
                {
                    return;
                }

                _api.PutAsync($"api/allocations/{allocToEnd.Id}/end").GetAwaiter().GetResult();

                Console.WriteLine();
                Console.WriteLine($"  Allocation ended. {selectedEmpName} freed from {selectedProject?.Name ?? "this project"} as of {todayStr}. ✓");
                Console.WriteLine($"  Employee status updated to BENCH if no other active allocations remain.");
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
    }
}
