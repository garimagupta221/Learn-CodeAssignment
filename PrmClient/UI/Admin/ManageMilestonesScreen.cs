using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class ManageMilestonesScreen : IScreen
    {
        private readonly ApiClient _api;

        public ManageMilestonesScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            try
            {
                var allProjects = _api.GetAsync<List<ProjectModel>>("api/projects")
                                      .GetAwaiter().GetResult() ?? new List<ProjectModel>();
                ManageProjectsScreen.PrintProjectTable(allProjects);
                Console.WriteLine();
            }
            catch (Exception)
            {
                /* Ignore if fetch fails */
            }

            int projectId = InputHelper.GetValidIntOption("  Enter Project ID: ", 1, int.MaxValue);

            Console.WriteLine();

            ProjectModel? project = null;
            try
            {
                project = _api.GetAsync<ProjectModel>($"api/projects/{projectId}")
                              .GetAwaiter().GetResult();
            }
            catch (HttpRequestException)
            {
                /* Likely a 404 */
            }

            if (project == null)
            {
                Console.WriteLine($"  Error: Project with ID {projectId} does not exist.");
                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
                AppState.CurrentScreen = "admin-projects";
                return;
            }

            List<MilestoneModel> milestones;
            try
            {
                milestones = _api.GetAsync<List<MilestoneModel>>($"api/milestones/project/{projectId}")
                                 .GetAwaiter().GetResult()
                             ?? new List<MilestoneModel>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching milestones: {ex.Message}");
                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
                AppState.CurrentScreen = "admin-projects";
                return;
            }

            Console.WriteLine($"  ── {project.Name} ───────────────────────────────");

            if (milestones.Count == 0)
            {
                Console.WriteLine("  No milestones found for this project.");
            }
            else
            {
                PrintMilestoneTable(milestones);

                int totalSP     = milestones.Sum(m => m.StoryPoints);
                int completedSP = milestones.Where(m => m.Status == "DONE").Sum(m => m.StoryPoints);
                int remainingSP = totalSP - completedSP;
                Console.WriteLine($"  Total: {totalSP} SP   |   Completed: {completedSP} SP   |   Remaining: {remainingSP} SP");
            }

            Console.WriteLine();
            Console.WriteLine("  1. Add Milestone");
            Console.WriteLine("  2. Update Milestone Status");
            Console.WriteLine("  3. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 3);

            switch (choice)
            {
                case 1: AddMilestone(projectId);    break;
                case 2: UpdateMilestone(milestones); break;
                case 3:
                    AppState.CurrentScreen = "admin-projects";
                    return;
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(intercept: true);
            AppState.CurrentScreen = "admin-milestones";
        }

        private void AddMilestone(int projectId)
        {
            Console.WriteLine();

            string   title       = InputHelper.GetRequiredString("  Milestone Title  : ");
            DateTime dueDate     = InputHelper.GetValidDate("  Due Date         : (DD-MM-YYYY) ");
            int      storyPoints = InputHelper.GetValidIntOption("  Story Points     : ", 0, int.MaxValue);

            try
            {
                _api.PostAsync<AddMilestoneRequest, MilestoneModel>(
                    $"api/projects/{projectId}/milestones",
                    new AddMilestoneRequest
                    {
                        Title       = title,
                        DueDate     = dueDate,
                        StoryPoints = storyPoints
                    }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Milestone added. ✓");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        private void UpdateMilestone(List<MilestoneModel> milestones)
        {
            Console.WriteLine();
            int number = InputHelper.GetValidIntOption("  Enter Milestone # : ", 1, milestones.Count);
            var target = milestones[number - 1];

            Console.Write("  New Status        : (1) NOT_STARTED   (2) IN_PROGRESS   (3) DONE → ");
            int statusChoice = InputHelper.GetValidIntOption("", 1, 3);
            string status = statusChoice switch { 1 => "NOT_STARTED", 2 => "IN_PROGRESS", _ => "DONE" };

            try
            {
                _api.PutAsync<UpdateMilestoneRequest, MilestoneModel>(
                    $"api/milestones/{target.Id}",
                    new UpdateMilestoneRequest { Status = status }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Milestone updated. ✓");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        private static void PrintMilestoneTable(List<MilestoneModel> milestones)
        {
            Console.WriteLine(
                $"  {"#",-4} {"Title",-20} {"Due Date",-12} {"Story Pts",-11} {"Status",-12}");
            Console.WriteLine(
                $"  {new string('─', 60)}");

            for (int i = 0; i < milestones.Count; i++)
            {
                var m = milestones[i];
                Console.WriteLine(
                    $"  {(i + 1) + ".",-4} {m.Title,-20} {m.DueDate,-12:dd-MM-yyyy} {m.StoryPoints,-11} {m.Status,-12}");
            }

            Console.WriteLine(
                $"  {new string('─', 60)}");
        }
    }
}
