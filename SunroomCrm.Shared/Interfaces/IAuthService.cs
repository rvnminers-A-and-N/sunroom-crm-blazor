using SunroomCrm.Shared.DTOs.Auth;

namespace SunroomCrm.Shared.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<UserDto> GetCurrentUserAsync();
}
