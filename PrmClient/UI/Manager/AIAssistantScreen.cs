using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class AIAssistantScreen : IScreen
    {
        private readonly ApiClient _api;

        public AIAssistantScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  AI Assistant – Project Risk Summary");
            Console.WriteLine();

            int projectId = InputHelper.GetValidIntOption("  Project ID: ", 1, int.MaxValue);

            try
            {
                var result = _api.GetAsync<AiResponseDto>($"api/ai/risk-summary/{projectId}")
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
            Console.Write("  Press Enter to return to menu...");
            Console.ReadLine();
            AppState.CurrentScreen = "manager-menu";
        }
    }
}
