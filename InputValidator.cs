using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    internal static class InputValidator
    {
        public static int GetValidChoiceInRange(int lowerBound, int upperBound)
        {
            while (true)
            {
                Console.Write($"Enter choice ({lowerBound}-{upperBound}): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= lowerBound && choice <= upperBound)
                {
                    return choice;
                }

                Console.WriteLine($"Invalid choice. Please enter {lowerBound}-{upperBound}");
            }
        }
        
        public static int GetValidPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Please enter a positive integer.");
            }
        }

        public static decimal GetValidDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Please enter a positive amount.");
            }
        }

        public static string GetValidUsername()
        {
            const int MIN_LENGTH = 3;

            while (true)
            {
                Console.Write("Enter Username: ");
                string username = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("Username cannot be empty.");
                    continue;
                }

                if (username.Length < MIN_LENGTH)
                {
                    Console.WriteLine($"Username must be at least {MIN_LENGTH} characters long.");
                    continue;
                }

                if (username.Length == MIN_LENGTH && IsAllSpecialCharacters(username))
                {
                    Console.WriteLine("Username cannot be only special characters.");
                    continue;
                }

                return username;
            }
        }

        private static bool IsAllSpecialCharacters(string input)
        {
            foreach (char character in input)
            {
                if (char.IsLetterOrDigit(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
