using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class MyProjectsScreen : IScreen
    {
        private readonly ApiClient _api;

        public MyProjectsScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  My Projects");
            Console.WriteLine();

            try
            {
                var projects = _api.GetAsync<List<ProjectModel>>($"api/projects/manager/{AppState.UserId}")
                                   .GetAwaiter().GetResult()
                               ?? new List<ProjectModel>();

                if (projects.Count == 0)
                {
                    Console.WriteLine("  No projects assigned to you.");
                }
                else
                {
                    PrintProjectTable(projects);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching projects: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("  1. View project details & milestones");
            Console.WriteLine("  2. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 2);

            if (choice == 1)
                DrillProjectDetails();

            AppState.CurrentScreen = "manager-menu";
        }

        private void DrillProjectDetails()
        {
            Console.WriteLine();
            int id = InputHelper.GetValidIntOption("  Enter Project ID: ", 1, int.MaxValue);

            try
            {
                var project = _api.GetAsync<ProjectModel>($"api/projects/{id}")
                                  .GetAwaiter().GetResult();

                if (project == null)
                {
                    Console.WriteLine("  Project not found.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"  {"ID:",-15} {project.Id}");
                    Console.WriteLine($"  {"Name:",-15} {project.Name}");
                    Console.WriteLine($"  {"Description:",-15} {project.Description}");
                    Console.WriteLine($"  {"Start Date:",-15} {project.StartDate:dd-MM-yyyy}");
                    Console.WriteLine($"  {"End Date:",-15} {project.EndDate:dd-MM-yyyy}");
                    Console.WriteLine($"  {"Status:",-15} {project.Status}");
                    Console.WriteLine($"  {"Health:",-15} {project.Health}");
                    Console.WriteLine();

                    var milestones = _api.GetAsync<List<MilestoneModel>>($"api/milestones/project/{id}")
                                        .GetAwaiter().GetResult()
                                    ?? new List<MilestoneModel>();

                    Console.WriteLine("  Milestones:");
                    if (milestones.Count == 0)
                    {
                        Console.WriteLine("    No milestones found.");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"  {"ID",-5} {"Title",-30} {"Due Date",-12} {"Status",-12}");
                        Console.WriteLine(
                            $"  {new string('─', 5),-5} {new string('─', 30),-30} {new string('─', 12),-12} {new string('─', 12),-12}");

                        foreach (var m in milestones)
                        {
                            Console.WriteLine($"  {m.Id,-5} {m.Title,-30} {m.DueDate,-12:dd-MM-yyyy} {m.Status,-12}");
                        }
                    }
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

        private static void PrintProjectTable(List<ProjectModel> projects)
        {
            Console.WriteLine(
                $"  {"ID",-5} {"Project Name",-35} {"End Date",-14} {"Health",-12} {"Status",-12}");
            Console.WriteLine(
                $"  {new string('─', 5),-5} {new string('─', 35),-35} {new string('─', 14),-14} {new string('─', 12),-12} {new string('─', 12),-12}");

            foreach (var p in projects)
            {
                Console.WriteLine($"  {p.Id,-5} {p.Name,-35} {p.EndDate,-14:dd-MM-yyyy} {p.Health,-12} {p.Status,-12}");
            }
        }
    }
}
