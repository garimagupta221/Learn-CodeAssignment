using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class ManagerMenu : IScreen
    {
        private readonly ApiClient _api;

        public ManagerMenu(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  Manager Panel");
            Console.WriteLine();
            Console.WriteLine("  1. Resource Dashboard");
            Console.WriteLine("  2. Allocate Resource");
            Console.WriteLine("  3. My Projects");
            Console.WriteLine("  4. Timesheets");
            Console.WriteLine("  5. AI Assistant");
            Console.WriteLine("  6. Frozen Timesheets");
            Console.WriteLine("  7. Logout");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 7);

            AppState.CurrentScreen = choice switch
            {
                1 => "manager-dashboard",
                2 => "manager-allocate",
                3 => "manager-projects",
                4 => "manager-timesheets",
                5 => "manager-ai",
                6 => "manager-frozen-timesheets",
                7 => "logout",
                _ => "manager-menu"
            };
        }
    }
}
