using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class SystemConfigScreen : IScreen
    {
        private readonly ApiClient _api;

        public SystemConfigScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("System Config");
            Console.WriteLine();
            Console.WriteLine("  1. View All Config");
            Console.WriteLine("  2. Set Config Value");
            Console.WriteLine("  3. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 3);

            switch (choice)
            {
                case 1: ViewConfig(); break;
                case 2: SetConfig();  break;
                case 3:
                    AppState.CurrentScreen = "admin-menu";
                    return;
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        private void ViewConfig()
        {
            Console.WriteLine();
            try
            {
                var config = _api.GetAsync<Dictionary<string, string>>("api/config")
                                 .GetAwaiter().GetResult()
                             ?? new Dictionary<string, string>();

                if (config.Count == 0)
                {
                    Console.WriteLine("  No config entries found.");
                    return;
                }

                Console.WriteLine($"  {"Key",-30} {"Value",-40}");
                Console.WriteLine($"  {new string('-', 30),-30} {new string('-', 40),-40}");
                foreach (var kv in config)
                    Console.WriteLine($"  {kv.Key,-30} {kv.Value,-40}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }

        private void SetConfig()
        {
            Console.WriteLine();
            string key   = InputHelper.GetRequiredString("  Config Key: ");
            string value = InputHelper.GetRequiredString("  Config Value: ");

            try
            {
                _api.PutAsync<SetConfigRequest>("api/config", new SetConfigRequest { Key = key, Value = value })
                    .GetAwaiter().GetResult();
                Console.WriteLine("\n  Config updated.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }
    }
}
