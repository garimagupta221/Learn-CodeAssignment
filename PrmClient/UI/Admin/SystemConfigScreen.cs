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

            var config = GetConfig();

            string llmProvider       = config.GetValueOrDefault("ActiveAiProvider", "Google Gemini");
            string llmApiKey         = config.GetValueOrDefault("AiApiKey", "");
            string maskedApiKey      = string.IsNullOrEmpty(llmApiKey) ? "" : new string('*', 28);
            string schedulerInterval = config.GetValueOrDefault("SchedulerInterval", "4 hours");
            string maxWeeklyHours    = config.GetValueOrDefault("MaxWeeklyHours", "40");

            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    SYSTEM CONFIGURATION                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Current Settings:");
            Console.WriteLine($"  LLM Provider        :  {llmProvider}");
            Console.WriteLine($"  LLM API Key         :  {maskedApiKey}");
            Console.WriteLine($"  Scheduler Interval  :  {schedulerInterval}");
            Console.WriteLine($"  Max Weekly Hours    :  {maxWeeklyHours}");
            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────");
            Console.WriteLine("1. Update LLM API Key");
            Console.WriteLine("2. Change LLM Provider  (Gemini / Groq)");
            Console.WriteLine("3. Update Scheduler Interval");
            Console.WriteLine("4. Update Max Weekly Hours");
            Console.WriteLine("5. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 5);

            switch (choice)
            {
                case 1:
                    UpdateConfigValue("AiApiKey", InputHelper.GetRequiredString("  New LLM API Key: "));
                    break;
                case 2:
                    Console.WriteLine("\n  Available Providers:");
                    Console.WriteLine("  1. Gemini");
                    Console.WriteLine("  2. Groq");
                    int providerChoice = InputHelper.GetValidIntOption("  Enter provider option: ", 1, 2);
                    string provider = providerChoice == 1 ? "Gemini" : "Groq";
                    UpdateConfigValue("ActiveAiProvider", provider);
                    break;
                case 3:
                    int interval = InputHelper.GetValidIntOption("  New Scheduler Interval (hours): ", 1, 168);
                    UpdateConfigValue("SchedulerInterval", interval.ToString());
                    break;
                case 4:
                    int hours = InputHelper.GetValidIntOption("  New Max Weekly Hours: ", 1, 168);
                    UpdateConfigValue("MaxWeeklyHours", hours.ToString());
                    break;
                case 5:
                    AppState.CurrentScreen = "admin-menu";
                    return;
            }
        }

        private Dictionary<string, string> GetConfig()
        {
            try
            {
                return _api.GetAsync<Dictionary<string, string>>("api/config")
                           .GetAwaiter().GetResult()
                       ?? new Dictionary<string, string>();
            }
            catch (HttpRequestException)
            {
                return new Dictionary<string, string>();
            }
        }

        private void UpdateConfigValue(string key, string value)
        {
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

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }
    }
}
