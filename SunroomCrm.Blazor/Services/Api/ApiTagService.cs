using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Tags;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiTagService : ITagService
{
    private readonly HttpClient _http;

    public ApiTagService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TagDto>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<TagDto>>("api/tags")
            ?? new List<TagDto>();
    }

    public async Task<TagDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<TagDto>($"api/tags/{id}");
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/tags", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TagDto>()
            ?? throw new InvalidOperationException("Invalid create response.");
    }

    public async Task<TagDto> UpdateAsync(int id, UpdateTagRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/tags/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TagDto>()
            ?? throw new InvalidOperationException("Invalid update response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/tags/{id}");
        response.EnsureSuccessStatusCode();
    }
}
