namespace PrmClient
{
    public static class AppState
    {
        public static string CurrentScreen { get; set; } = "start";
        public static string Role { get; set; } = "Guest";
        public static int UserId { get; set; }
        public static string Username { get; set; } = string.Empty;
        public static string FullName { get; set; } = string.Empty;
    }
}
