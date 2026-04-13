using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiActivityService : IActivityService
{
    private readonly HttpClient _http;

    public ApiActivityService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResponse<ActivityDto>> GetAllAsync(ActivityFilterParams filter)
    {
        var query = $"api/activities?page={filter.Page}&perPage={filter.PerPage}";
        if (filter.ContactId.HasValue) query += $"&contactId={filter.ContactId}";
        if (filter.DealId.HasValue) query += $"&dealId={filter.DealId}";
        if (!string.IsNullOrEmpty(filter.Type)) query += $"&type={filter.Type}";
        if (!string.IsNullOrEmpty(filter.Sort)) query += $"&sort={filter.Sort}";
        if (!string.IsNullOrEmpty(filter.Direction)) query += $"&direction={filter.Direction}";

        return await _http.GetFromJsonAsync<PaginatedResponse<ActivityDto>>(query)
            ?? new PaginatedResponse<ActivityDto>();
    }

    public async Task<ActivityDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ActivityDto>($"api/activities/{id}");
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/activities", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ActivityDto>()
            ?? throw new InvalidOperationException("Invalid create response.");
    }

    public async Task<ActivityDto> UpdateAsync(int id, UpdateActivityRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/activities/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ActivityDto>()
            ?? throw new InvalidOperationException("Invalid update response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/activities/{id}");
        response.EnsureSuccessStatusCode();
    }
}
