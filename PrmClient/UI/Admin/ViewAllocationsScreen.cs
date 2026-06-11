using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            string filterText = "";
            
            while(true)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.PrintHeader(AppState.Role);
                Console.WriteLine("  ╔══════════════════════════════════════════════╗");
                Console.WriteLine("  ║    ALL ALLOCATIONS                           ║");
                Console.WriteLine("  ╚══════════════════════════════════════════════╝");
                Console.WriteLine();

                try
                {
                    var allocations = _api.GetAsync<List<AllocationModel>>("api/allocations")
                                          .GetAwaiter().GetResult()
                                      ?? new List<AllocationModel>();

                    var activeAllocations = allocations.Where(a => a.IsActive).ToList();

                    var employees = _api.GetAsync<List<EmployeeModel>>("api/employees").GetAwaiter().GetResult() ?? new List<EmployeeModel>();
                    var projects = _api.GetAsync<List<ProjectModel>>("api/projects").GetAwaiter().GetResult() ?? new List<ProjectModel>();

                    var displayRows = activeAllocations.Select(a => {
                        var emp = employees.FirstOrDefault(e => e.Id == a.EmployeeId);
                        var proj = projects.FirstOrDefault(p => p.Id == a.ProjectId);
                        return new {
                            Alloc = a,
                            EmpName = emp?.FullName ?? $"Employee {a.EmployeeId}",
                            ProjName = proj?.Name ?? $"Project {a.ProjectId}"
                        };
                    }).ToList();

                    if (!string.IsNullOrWhiteSpace(filterText))
                    {
                        var lowerFilter = filterText.ToLower();
                        displayRows = displayRows.Where(r => r.EmpName.ToLower().Contains(lowerFilter) || r.ProjName.ToLower().Contains(lowerFilter)).ToList();
                        Console.WriteLine($"  [Filtered by: '{filterText}']");
                        Console.WriteLine();
                    }

                    if (displayRows.Count == 0)
                    {
                        Console.WriteLine("  No active allocations found.");
                    }
                    else
                    {
                        Console.WriteLine($"  {"Employee",-18} {"Project",-18} {"%",-6} {"From",-12} {"To"}");
                        Console.WriteLine($"  {new string('─', 62)}");
                        foreach (var row in displayRows)
                        {
                            Console.WriteLine($"  {row.EmpName,-18} {row.ProjName,-18} {$"{row.Alloc.UtilizationPct}%",-6} {row.Alloc.StartDate,-12:dd-MM-yyyy} {row.Alloc.EndDate:dd-MM-yyyy}");
                        }
                        Console.WriteLine($"  {new string('─', 62)}");
                        Console.WriteLine($"  Total Active Allocations: {displayRows.Count}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"  Error: {ex.Message}");
                }

                Console.WriteLine();
                Console.Write("  [F] Filter by Employee / Project     [B] Back > ");
                var key = Console.ReadLine()?.Trim().ToUpper();
                if (key == "B")
                {
                    break;
                }
                else if (key == "F")
                {
                    Console.Write("  Enter filter text: ");
                    filterText = Console.ReadLine()?.Trim() ?? "";
                }
            }
            
            AppState.CurrentScreen = "admin-menu";
        }
    }
}
