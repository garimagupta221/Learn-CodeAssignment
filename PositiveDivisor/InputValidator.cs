using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PositiveDivisor
{
    public class InputValidator
    {
        public void ValidateNumber(int number)
        {
            if (number <= 0)
            {
                throw new ArgumentException("Number must be greater than 0");
            }
        }

        public void ValidateLimit(int limit)
        {
            if (limit <= 2)
            {
                throw new ArgumentException("Limit must be greater than 2");
            }    
        }

        public int ReadValidInput()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out int value) && value > 0)
                {
                    return value;
                }
                Console.WriteLine("Invalid input. Enter a valid positive integer");
            }
        }

        public int ReadValidInt(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine();

                if (int.TryParse(input, out int value))
                {
                    return value;
                }

                Console.WriteLine("Invalid input. Please enter a valid integer");
            }
        }

    }
}
