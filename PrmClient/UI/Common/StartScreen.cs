namespace PrmClient.UI.Common
{
    public class StartScreen : IScreen
    {
        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader("Guest");
            Console.WriteLine("Welcome to PRM Tool");
            Console.WriteLine();
            Console.WriteLine("  1. Login");
            Console.WriteLine("  2. Exit");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 2);

            AppState.CurrentScreen = choice switch
            {
                1 => "login",
                2 => "exit",
                _ => "start"
            };
        }
    }
}
