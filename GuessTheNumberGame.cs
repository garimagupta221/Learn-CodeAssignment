using System;
namespace FunctionAssignment
{
    public class GuessNumberGame
    {
        static void Main(string[] args)
        {
            PlayGame();
        }

        static void PlayGame()
        {
            int targetNumber = GenerateTargetNumber();
            int guessCount = 0;

            while (true)
            {
                int guess = ReadValidGuessNumber();
                guessCount++;

                if (guess == targetNumber)
                {
                    Console.WriteLine($"You guessed it in {guessCount} guesses");
                    return;
                }

                Console.WriteLine(guess < targetNumber ? "Too low" : "Too high");
            }
        }

        static int GenerateTargetNumber()
        {
            return new Random().Next(1, 101);
        }

        static int ReadValidGuessNumber()
        {
            while (true)
            {
                Console.Write("Guess a number between 1 and 100: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int guess) && guess >= 1 &&guess <= 100)
                {
                    return guess;
                }

                Console.WriteLine("Invalid input. Enter a number between 1 and 100");
            }
        }
    }
}
