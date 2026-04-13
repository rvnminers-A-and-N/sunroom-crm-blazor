using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Deals;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiDealService : IDealService
{
    private readonly HttpClient _http;

    public ApiDealService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResponse<DealDto>> GetAllAsync(DealFilterParams filter)
    {
        var query = $"api/deals?page={filter.Page}&perPage={filter.PerPage}";
        if (!string.IsNullOrEmpty(filter.Search)) query += $"&search={Uri.EscapeDataString(filter.Search)}";
        if (!string.IsNullOrEmpty(filter.Stage)) query += $"&stage={filter.Stage}";
        if (!string.IsNullOrEmpty(filter.Sort)) query += $"&sort={filter.Sort}";
        if (!string.IsNullOrEmpty(filter.Direction)) query += $"&direction={filter.Direction}";

        return await _http.GetFromJsonAsync<PaginatedResponse<DealDto>>(query)
            ?? new PaginatedResponse<DealDto>();
    }

    public async Task<DealDetailDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<DealDetailDto>($"api/deals/{id}");
    }

    public async Task<DealDto> CreateAsync(CreateDealRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/deals", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DealDto>()
            ?? throw new InvalidOperationException("Invalid create response.");
    }

    public async Task<DealDto> UpdateAsync(int id, UpdateDealRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/deals/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DealDto>()
            ?? throw new InvalidOperationException("Invalid update response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/deals/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<PipelineDto> GetPipelineAsync()
    {
        return await _http.GetFromJsonAsync<PipelineDto>("api/deals/pipeline")
            ?? new PipelineDto();
    }
}
