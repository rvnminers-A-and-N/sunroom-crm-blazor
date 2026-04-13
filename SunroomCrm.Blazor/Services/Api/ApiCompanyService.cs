using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Companies;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiCompanyService : ICompanyService
{
    private readonly HttpClient _http;

    public ApiCompanyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResponse<CompanyDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = $"api/companies?page={pagination.Page}&perPage={pagination.PerPage}";
        if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(pagination.Sort)) query += $"&sort={pagination.Sort}";
        if (!string.IsNullOrEmpty(pagination.Direction)) query += $"&direction={pagination.Direction}";

        return await _http.GetFromJsonAsync<PaginatedResponse<CompanyDto>>(query)
            ?? new PaginatedResponse<CompanyDto>();
    }

    public async Task<CompanyDetailDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<CompanyDetailDto>($"api/companies/{id}");
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/companies", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompanyDto>()
            ?? throw new InvalidOperationException("Invalid create response.");
    }

    public async Task<CompanyDto> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/companies/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompanyDto>()
            ?? throw new InvalidOperationException("Invalid update response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/companies/{id}");
        response.EnsureSuccessStatusCode();
    }
}
