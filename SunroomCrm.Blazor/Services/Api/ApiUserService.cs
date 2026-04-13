using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Auth;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiUserService : IUserService
{
    private readonly HttpClient _http;

    public ApiUserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<UserDto>>("api/users")
            ?? new List<UserDto>();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<UserDto>($"api/users/{id}");
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Invalid update response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
    }
}
