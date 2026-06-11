using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class ManageProjectsScreen : IScreen
    {
        private readonly ApiClient _api;

        public ManageProjectsScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  1. Create Project");
            Console.WriteLine("  2. View All Projects");
            Console.WriteLine("  3. Update Project Details");
            Console.WriteLine("  4. Manage Milestones");
            Console.WriteLine("  5. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 5);

            switch (choice)
            {
                case 1: CreateProject();      break;
                case 2: ViewAllProjects();    break;
                case 3: UpdateProjectDetails(); break;
                case 4:
                    AppState.CurrentScreen = "admin-milestones";
                    return;
                case 5:
                    AppState.CurrentScreen = "admin-menu";
                    return;
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        // ─── Screen 3.2.1 — Create Project ───────────────────────────────────

        private void CreateProject()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    CREATE PROJECT                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            string name        = InputHelper.GetRequiredString("  Project Name        : ");
            string description = InputHelper.GetRequiredString("  Description         : ");
            DateTime startDate = InputHelper.GetValidDate("  Start Date          : (DD-MM-YYYY) ");
            DateTime endDate   = InputHelper.GetValidDate("  End Date            : (DD-MM-YYYY) ");

            Console.Write("  Status              : (1) PLANNED   (2) ACTIVE   (3) ON_HOLD  ");
            int statusChoice = InputHelper.GetValidIntOption("", 1, 3);
            string status = statusChoice switch { 1 => "PLANNED", 2 => "ACTIVE", _ => "ON_HOLD" };

            int managerId       = InputHelper.GetValidIntOption("  Assign Manager      : (Enter Manager ID) ", 1, int.MaxValue);
            int totalSP         = InputHelper.GetValidIntOption("  Total Story Points  : ", 0, int.MaxValue);

            Console.WriteLine();
            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.Write("  [S] Save     [B] Back → ");

            string action = Console.ReadLine()?.Trim().ToUpper() ?? "B";
            if (action != "S")
            {
                return;
            }

            try
            {
                var project = _api.PostAsync<CreateProjectRequest, ProjectModel>(
                    "api/projects",
                    new CreateProjectRequest
                    {
                        Name             = name,
                        Description      = description,
                        StartDate        = startDate,
                        EndDate          = endDate,
                        Status           = status,
                        ManagerId        = managerId,
                        TotalStoryPoints = totalSP
                    }
                ).GetAwaiter().GetResult();

                Console.WriteLine($"\n  Project '{project?.Name}' created successfully (ID: {project?.Id}).");
                System.Threading.Thread.Sleep(1500);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
            }
        }

        // ─── Screen 3.2.2 — View All Projects ────────────────────────────────

        private void ViewAllProjects()
        {
            Console.WriteLine();
            try
            {
                var projects = _api.GetAsync<List<ProjectModel>>("api/projects")
                                   .GetAwaiter().GetResult()
                               ?? new List<ProjectModel>();

                if (projects.Count == 0)
                {
                    Console.WriteLine("  No projects found.");
                    return;
                }

                PrintProjectTable(projects);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }

        // ─── Screen 3.2.3 — Update Project Details ───────────────────────────

        private void UpdateProjectDetails()
        {
            Console.WriteLine();
            
            try
            {
                var allProjects = _api.GetAsync<List<ProjectModel>>("api/projects")
                                      .GetAwaiter().GetResult() ?? new List<ProjectModel>();
                PrintProjectTable(allProjects);
                Console.WriteLine();
            }
            catch (Exception)
            {
                // Ignore if fetch fails and proceed to prompt
            }

            int id = InputHelper.GetValidIntOption("  Enter Project ID: ", 1, int.MaxValue);

            ProjectModel? current;
            try
            {
                current = _api.GetAsync<ProjectModel>($"api/projects/{id}")
                              .GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error fetching project: {ex.Message}");
                return;
            }

            if (current == null)
            {
                Console.WriteLine("\n  Project not found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"  ── {current.Name} ───────────────────────────────");

            string name = InputHelper.GetRequiredString($"  Project Name         : {current.Name,-30}  (editable) → ");
            string description = InputHelper.GetRequiredString($"  Description          : {current.Description,-30}  (editable) → ");
            DateTime startDate = InputHelper.GetValidDate($"  Start Date           : {current.StartDate,-15:dd-MMM-yy}  (editable) → (DD-MM-YYYY) ");
            DateTime endDate   = InputHelper.GetValidDate($"  End Date             : {current.EndDate,-15:dd-MMM-yy}  (editable) → (DD-MM-YYYY) ");

            Console.Write("  Status               : (1) PLANNED   (2) ACTIVE   (3) ON_HOLD   (4) COMPLETED → ");
            int statusChoice = InputHelper.GetValidIntOption("", 1, 4);
            string status = statusChoice switch { 1 => "PLANNED", 2 => "ACTIVE", 3 => "ON_HOLD", _ => "COMPLETED" };

            int managerId = InputHelper.GetValidIntOption($"  Assign Manager       : (Enter Manager ID) [{current.ManagerId}] → ", 1, int.MaxValue);
            int totalSP   = InputHelper.GetValidIntOption($"  Total Story Points   : {current.TotalStoryPoints,-10}  (editable) → ", 0, int.MaxValue);

            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.Write("  [S] Save     [B] Back → ");
            string action = Console.ReadLine()?.Trim().ToUpper() ?? "B";
            if (action != "S")
            {
                Console.WriteLine("  Cancelled.");
                return;
            }

            try
            {
                var updated = _api.PutAsync<UpdateProjectRequest, ProjectModel>(
                    $"api/projects/{id}",
                    new UpdateProjectRequest
                    {
                        Name             = name,
                        Description      = description,
                        StartDate        = startDate,
                        EndDate          = endDate,
                        Status           = status,
                        ManagerId        = managerId,
                        TotalStoryPoints = totalSP
                    }
                ).GetAwaiter().GetResult();

                Console.WriteLine($"\n  Project '{updated?.Name}' updated successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Table printer ────────────────────────────────────────────────────

        public static void PrintProjectTable(List<ProjectModel> projects)
        {
            Console.WriteLine(
                $"  {"ID",-5} {"Name",-18} {"Manager",-15} {"End Date",-12} {"Status",-10} {"SP Done/Total",-13}");
            Console.WriteLine(
                $"  {new string('─', 78)}");

            foreach (var p in projects)
            {
                string spCol = $"{p.CompletedStoryPoints} / {p.TotalStoryPoints}";
                string managerDisplay = string.IsNullOrEmpty(p.ManagerName) ? p.ManagerId.ToString() : p.ManagerName;
                if (managerDisplay.Length > 14) managerDisplay = managerDisplay.Substring(0, 14);

                Console.WriteLine(
                    $"  {p.Id,-5} {p.Name,-18} {managerDisplay,-15} {p.EndDate,-12:dd-MMM-yy} {p.Status,-10} {spCol,-13}");
            }

            Console.WriteLine(
                $"  {new string('─', 78)}");
        }
    }
}

