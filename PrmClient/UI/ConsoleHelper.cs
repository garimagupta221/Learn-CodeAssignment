namespace PrmClient.UI
{
    public static class ConsoleHelper
    {
        public static void ClearScreen()
        {
            try
            {
                Console.Clear();
            }
            catch (System.IO.IOException)
            {
                // Ignore console clear failure when stdout/stdin is redirected in test/background tasks
            }
        }

        public static void PrintHeader(string role, DateTime? date = null)
        {
            DateTime displayDate = date ?? DateTime.Now;
            Console.WriteLine($"Role: {role}");
            Console.WriteLine($"Date: {displayDate:dddd, MMMM d, yyyy}");
            Console.WriteLine();
        }
    }
}
