namespace PrmClient.UI
{
    public static class InputHelper
    {
        public static string GetRequiredString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();

                Console.WriteLine("  This field cannot be empty. Please try again.");
            }
        }

        public static string GetValidEmail(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    int atIndex = input.IndexOf('@');
                    if (atIndex > 0 && atIndex < input.Length - 2 && input.LastIndexOf('.') > atIndex + 1)
                        return input;
                }

                Console.WriteLine("  Invalid email. Must contain '@' and a valid domain (e.g. user@example.com).");
            }
        }

        public static string GetValidPassword(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string password = ReadMaskedInput();
                if (password.Length >= 8
                    && password.Any(char.IsUpper)
                    && password.Any(char.IsDigit))
                    return password;

                Console.WriteLine("  Password must be at least 8 characters, include 1 uppercase letter and 1 number.");
            }
        }

        public static DateTime GetValidDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (DateTime.TryParse(input, out DateTime value))
                    return value;

                Console.WriteLine("  Invalid date. Please use format yyyy-MM-dd (e.g. 2025-12-31).");
            }
        }

        public static int GetValidIntOption(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;

                Console.WriteLine($"  Invalid option. Please enter a number between {min} and {max}.");
            }
        }

        public static float GetValidFloat(string prompt, float min, float max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (float.TryParse(input, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float value)
                    && value >= min && value <= max)
                    return value;

                Console.WriteLine($"  Invalid value. Please enter a number between {min} and {max}.");
            }
        }

        private static string ReadMaskedInput()
        {
            string result = string.Empty;
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && result.Length > 0)
                {
                    result = result[..^1];
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    result += key.KeyChar;
                    Console.Write('*');
                }
            }
            return result;
        }
    }
}
