using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class AdminMenu : IScreen
    {
        private readonly ApiClient _api;

        public AdminMenu(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();

            string name = string.IsNullOrWhiteSpace(AppState.Username) ? "Admin" : AppState.Username;
            string dateTime = DateTime.Now.ToString("dd-MM-yyyy  HH:mm");
            string welcomeInner = ("    Welcome, " + name.PadRight(9) + "  |  " + dateTime).PadRight(46);

            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    ADMIN PANEL                               ║");
            Console.WriteLine("║" + welcomeInner + "║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("1. Manage Employees");
            Console.WriteLine("2. Manage Projects");
            Console.WriteLine("3. View All Allocations");
            Console.WriteLine("4. Manage Users");
            Console.WriteLine("5. System Configuration");
            Console.WriteLine("6. Logout");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 6);

            AppState.CurrentScreen = choice switch
            {
                1 => "admin-employees",
                2 => "admin-projects",
                3 => "admin-allocations",
                4 => "admin-users",
                5 => "admin-config",
                6 => "logout",
                _ => "admin-menu"
            };
        }
    }
}
