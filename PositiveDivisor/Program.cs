using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PositiveDivisor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var validator = new InputValidator();
            var calculator = new DivisorCalculator(validator);

            Console.WriteLine("Divisor Calculator");
            Console.Write("Enter number of test cases: ");

            int testCases = validator.ReadValidInput();
            for (int index = 0; index < testCases; index++)
            {
                int limit = validator.ReadValidInt($"Enter limit for test case {index + 1}: ");
                try
                {
                    int result = calculator.GetTotalDivisors(limit);
                    Console.WriteLine($"Result: {result}");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

        }
    }
}
