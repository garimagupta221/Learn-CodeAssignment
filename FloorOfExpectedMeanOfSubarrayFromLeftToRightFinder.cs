using System;
namespace FunctionAssignment
{
    public class Program
    {
        static void Main(string[] args)
        {
            int elementCount = GetUserInput("Enter number of elements: ");
            int queryCount = GetUserInput("Enter number of queries: ");

            Console.WriteLine($"Enter {elementCount} space-separated array elements:");
            long[] arrayElements = ReadLongArray();

            long[] prefixSumArray = CreatePrefixSumArray(arrayElements, elementCount);

            Console.WriteLine("Enter each query as two space-separated indices (L R):");
            ProcessQuery(queryCount, prefixSumArray);
        }
        private static int GetUserInput(string message)
        {
            while (true)
            {
                Console.Write(message);

                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Value must be a positive number. Try again.");
            }
        }

        private static int[] ReadIntArray()
        {
            return Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
        }

        private static long[] ReadLongArray()
        {
            return Array.ConvertAll(Console.ReadLine().Split(), long.Parse);
        }

        private static long[] CreatePrefixSumArray(long[] arrayElements, int elementCount)
        {
            long[] prefixSumArray = new long[elementCount + 1];

            for (int index = 1; index <= elementCount; index++)
            {
                prefixSumArray[index] =
                    prefixSumArray[index - 1] + arrayElements[index - 1];
            }

            return prefixSumArray;
        }

        private static void ProcessQuery(int queryCount, long[] prefixSumArray)
        {
            for (int queryIndex = 0; queryIndex < queryCount; queryIndex++)
            {
                Console.Write($"Query {queryIndex + 1}: ");
                int[] range = ReadIntArray();
                int leftIndex = range[0];
                int rightIndex = range[1];

                long subarraySum =
                    prefixSumArray[rightIndex] - prefixSumArray[leftIndex - 1];

                int subarrayLength = rightIndex - leftIndex + 1;

                long floorMean = subarraySum / subarrayLength;
                Console.WriteLine($"Floor Mean: {floorMean}");
            }
        }
    }
}
