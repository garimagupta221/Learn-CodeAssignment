using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class ManageUsersScreen : IScreen
    {
        private readonly ApiClient _api;

        public ManageUsersScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    MANAGE USERS                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("  1. Create User Account");
            Console.WriteLine("  2. View All Users");
            Console.WriteLine("  3. Reset User Password");
            Console.WriteLine("  4. Deactivate User");
            Console.WriteLine("  5. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 5);

            switch (choice)
            {
                case 1:
                    CreateUserAccount();
                    return;  // CreateUserAccount handles its own keypress pause
                case 2: ViewAllUsers();          break;
                case 3: ResetUserPassword();     return;
                case 4: DeactivateUser();        return;
                case 5:
                    AppState.CurrentScreen = "admin-menu";
                    return;
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        // ─── Create User Account (BRD Screen 3.4.1) ──────────────────────────

        private void CreateUserAccount()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    CREATE USER ACCOUNT                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.Write("Full Name          : ");
            string fullName = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Email              : ");
            string email = Console.ReadLine()?.Trim() ?? string.Empty;

            Console.Write("Username           : ");
            string username = Console.ReadLine()?.Trim() ?? string.Empty;

            string password = string.Empty;
            while (true)
            {
                Console.Write("Temporary Password : ");
                password = Console.ReadLine()?.Trim() ?? string.Empty;
                if (password.Length >= 8
                    && password.Any(char.IsUpper)
                    && password.Any(char.IsDigit))
                    break;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Password must be 8+ characters with at least one uppercase letter and one number.");
                Console.ResetColor();
            }

            Console.WriteLine("Role               : (1) Admin  (2) Manager  (3) Engineer");
            Console.Write("                     ");
            string role = string.Empty;
            while (string.IsNullOrEmpty(role))
            {
                string? roleInput = Console.ReadLine()?.Trim();
                role = roleInput switch
                {
                    "1" => "Admin",
                    "2" => "Manager",
                    "3" => "Engineer",
                    _   => string.Empty
                };
                if (string.IsNullOrEmpty(role))
                {
                    Console.Write("  Invalid choice. Enter 1, 2, or 3: ");
                }
            }

            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────");
            Console.WriteLine("[S] Save     [B] Back");

            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.B)
                    return;

                if (key == ConsoleKey.S)
                {
                    try
                    {
                        _api.PostAsync<SignUpRequest, UserModel>(
                            "api/auth/signup",
                            new SignUpRequest
                            {
                                FullName = fullName,
                                Email    = email,
                                Username = username,
                                Password = password,
                                Role     = role
                            }
                        ).GetAwaiter().GetResult();

                        Console.WriteLine();
                        Console.WriteLine("Account created. User must change password on first login. ✓");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ResetColor();
                    }

                    Console.WriteLine();
                    Console.WriteLine("  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }
            }
        }

        // ─── View All Users (BRD Screen 3.4.2) ───────────────────────────────

        private void ViewAllUsers()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    ALL USERS                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            try
            {
                var users = _api.GetAsync<List<UserModel>>("api/users")
                                .GetAwaiter().GetResult()
                            ?? new List<UserModel>();

                if (users.Count == 0)
                {
                    Console.WriteLine("  No users found.");
                    return;
                }

                PrintUserTable(users);

                Console.WriteLine();
                Console.WriteLine("[R] Reactivate a user     [B] Back");

                while (true)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.B)
                        return;
                    if (key == ConsoleKey.R)
                    {
                        ReactivateUser(users);
                        return;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }

        // ─── Reactivate User (inline from View All Users, BRD Screen 3.4.2) ──

        private void ReactivateUser(List<UserModel> users)
        {
            Console.WriteLine();
            int id = InputHelper.GetValidIntOption("  Enter User ID to reactivate: ", 1, int.MaxValue);

            // ── Client-side guard: check the already-fetched list before hitting the API ──
            var match = users.FirstOrDefault(u => u.Id == id);
            if (match is not null && match.IsActive)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Error: User is already active.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"  Reactivate this account?");
            Console.WriteLine("  [Y] Yes     [B] Cancel");

            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.B)
                    return;
                if (key == ConsoleKey.Y)
                {
                    try
                    {
                        _api.PutAsync<object, object>($"api/users/{id}/activate", new { })
                            .GetAwaiter().GetResult();
                        Console.WriteLine();
                        Console.WriteLine("  Account reactivated. ✓");
                        Console.WriteLine("  Note: Previous allocations are NOT restored. Admin must re-allocate manually if needed.");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n  Error: {ex.Message}");
                        Console.ResetColor();
                    }
                    return;
                }
            }
        }

        // ─── Reset User Password (BRD Screen 3.4.3) ──────────────────────────

        private void ResetUserPassword()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    RESET USER PASSWORD                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.Write("  Enter Username or User ID: ");
            string identifier = Console.ReadLine()?.Trim() ?? string.Empty;

            UserModel? user = null;
            try
            {
                // Try lookup by username or ID
                user = _api.GetAsync<UserModel>($"api/users/lookup/{identifier}")
                           .GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n  User not found: {ex.Message}");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"  User found: {user?.Username} ({user?.Role})");
            Console.WriteLine();

            string newPassword = string.Empty;
            while (true)
            {
                Console.Write("  New Temporary Password: ");
                newPassword = Console.ReadLine()?.Trim() ?? string.Empty;
                if (newPassword.Length >= 8
                    && newPassword.Any(char.IsUpper)
                    && newPassword.Any(char.IsDigit))
                    break;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Password must be 8+ characters with at least one uppercase letter and one number.");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────");
            Console.WriteLine("[S] Save     [B] Back");

            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.B)
                    return;
                if (key == ConsoleKey.S)
                {
                    try
                    {
                        _api.PutAsync<object, object>(
                            $"api/users/{user?.Id}/reset-password",
                            new { Password = newPassword }
                        ).GetAwaiter().GetResult();

                        Console.WriteLine();
                        Console.WriteLine("  Password reset. User will be prompted to change it on next login. ✓");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  Error: {ex.Message}");
                        Console.ResetColor();
                    }
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(intercept: true);
                    }
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }
            }
        }

        // ─── Deactivate User (BRD Screen 3.4.4) ──────────────────────────────

        private void DeactivateUser()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    DEACTIVATE USER                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.Write("  Enter Username or User ID: ");
            string identifier = Console.ReadLine()?.Trim() ?? string.Empty;

            UserModel? user = null;
            try
            {
                user = _api.GetAsync<UserModel>($"api/users/lookup/{identifier}")
                           .GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n  User not found: {ex.Message}");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"  User found: {user?.Username} ({user?.Role})");
            Console.WriteLine($"  Status     : {(user?.IsActive == true ? "Active" : "Inactive")}");
            Console.WriteLine();
            Console.WriteLine("  Are you sure you want to deactivate this account?");
            Console.WriteLine("  Deactivated users cannot log in. Their data is preserved.");
            Console.WriteLine();
            Console.WriteLine("  [Y] Yes, Deactivate     [B] Back");

            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.B)
                    return;
                if (key == ConsoleKey.Y)
                {
                    try
                    {
                        _api.PutAsync<object, object>($"api/users/{user?.Id}/deactivate", new { })
                            .GetAwaiter().GetResult();
                        Console.WriteLine();
                        Console.WriteLine("  User deactivated. ✓");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"\n  Error: {ex.Message}");
                    }
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(intercept: true);
                    }
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }
            }
        }

        // ─── Table printer ────────────────────────────────────────────────────

        private static void PrintUserTable(List<UserModel> users)
        {
            Console.WriteLine("ID    Username          Role        Status");
            Console.WriteLine("──────────────────────────────────────────────");

            foreach (var u in users)
            {
                Console.WriteLine($"{u.Id,-5} {u.Username,-17} {u.Role.ToUpper(),-11} {(u.IsActive ? "Active" : "Inactive")}");
            }
            
            Console.WriteLine("──────────────────────────────────────────────");

            int active   = users.Count(u => u.IsActive);
            int inactive = users.Count - active;
            Console.WriteLine($"Total: {users.Count}   |   Active: {active}   |   Inactive: {inactive}");
        }


    }
}
