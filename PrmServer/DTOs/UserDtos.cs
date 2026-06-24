namespace PrmServer.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsTemporaryPassword { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
}
