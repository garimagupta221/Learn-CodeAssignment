using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Common
{
    public class LoginScreen : IScreen
    {
        private readonly ApiClient _api;

        public LoginScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader("Guest");
            Console.WriteLine("Login");
            Console.WriteLine();

            string username = InputHelper.GetRequiredString("Username: ");
            string password = InputHelper.GetValidPassword("Password: ");

            try
            {
                var response = _api.PostAsync<LoginRequest, LoginResponse>(
                    "api/auth/login",
                    new LoginRequest { Username = username, Password = password }
                ).GetAwaiter().GetResult();

                if (response is null)
                {
                    Console.WriteLine("\n  Login failed. Please try again.");
                    Console.ReadKey(intercept: true);
                    AppState.CurrentScreen = "login";
                    return;
                }

                _api.SetToken(response.Token);
                AppState.UserId = response.UserId;
                AppState.Role = response.Role;
                AppState.Username = username;

                Console.WriteLine($"\n  Welcome! Logged in as {response.Role}.");

                if (response.IsTemporaryPassword)
                {
                    Console.WriteLine("  You must change your password before continuing.");
                    Console.ReadKey(intercept: true);
                    AppState.CurrentScreen = "change-password";
                    return;
                }

                AppState.CurrentScreen = response.Role.ToLower() switch
                {
                    "admin"    => "admin-menu",
                    "manager"  => "manager-menu",
                    "engineer" => "employee-menu",
                    _          => "login"
                };
            }
            catch (Exception)
            {
                Console.WriteLine("\n  Error: Invalid username or password.");
                Console.WriteLine("  Press any key to return...");
                Console.ReadKey();
                AppState.CurrentScreen = "start";
            }
        }
    }
}

