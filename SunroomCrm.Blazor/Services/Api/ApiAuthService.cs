using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Auth;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiAuthService : IAuthService
{
    private readonly HttpClient _http;

    public ApiAuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Invalid login response.");
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Invalid register response.");
    }

    public async Task<UserDto> GetCurrentUserAsync()
    {
        return await _http.GetFromJsonAsync<UserDto>("api/auth/me")
            ?? throw new UnauthorizedAccessException("Failed to get current user.");
    }
}
