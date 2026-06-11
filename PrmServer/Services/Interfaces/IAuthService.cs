using PrmServer.DTOs;

namespace PrmServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<UserDto> SignUpAsync(SignUpDto dto);
        Task LogoutAsync(string token);
        Task ChangePasswordAsync(ChangePasswordDto dto);
    }
}
