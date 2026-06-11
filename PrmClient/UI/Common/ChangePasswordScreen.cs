using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Common
{
    public class ChangePasswordScreen : IScreen
    {
        private readonly ApiClient _api;

        public ChangePasswordScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            // This screen cannot be skipped — loop until password is successfully changed
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════════════════════════╗");
                Console.WriteLine("║    CHANGE PASSWORD                           ║");
                Console.WriteLine("║    You must set a new password to continue.  ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");
                Console.WriteLine();

                string newPassword     = InputHelper.GetValidPassword("New Password        : ");
                string confirmPassword = InputHelper.GetValidPassword("Confirm Password    : ");

                if (newPassword != confirmPassword)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n  Passwords do not match. Please try again.");
                    Console.ResetColor();
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    continue;   // loop back — screen cannot be skipped
                }

                Console.WriteLine();
                Console.WriteLine("──────────────────────────────────────────────");
                Console.WriteLine("[S] Save and Continue");

                // Wait for S (any other key is silently ignored)
                while (true)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key != ConsoleKey.S) continue;
                    break;
                }

                try
                {
                    _api.PostAsync<ChangePasswordRequest>(
                        "api/auth/change-password",
                        new ChangePasswordRequest
                        {
                            UserId      = AppState.UserId,
                            NewPassword = newPassword
                        }
                    ).GetAwaiter().GetResult();

                    Console.WriteLine();
                    Console.WriteLine("Password updated. Welcome! ✓");
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);

                    // Navigate to the correct role menu after password change
                    AppState.CurrentScreen = AppState.Role.ToLower() switch
                    {
                        "admin"    => "admin-menu",
                        "manager"  => "manager-menu",
                        "engineer" => "employee-menu",
                        _          => "login"
                    };
                    return;
                }
                catch (HttpRequestException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n  Error: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("\n  Press any key to try again...");
                    Console.ReadKey(intercept: true);
                    // loop back — screen cannot be skipped
                }
            }
        }
    }
}
