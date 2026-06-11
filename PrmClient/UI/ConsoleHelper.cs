namespace PrmClient.UI
{
    public static class ConsoleHelper
    {
        public static void ClearScreen()
        {
            Console.Clear();
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
