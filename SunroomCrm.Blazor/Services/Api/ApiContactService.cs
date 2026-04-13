using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Contacts;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiContactService : IContactService
{
    private readonly HttpClient _http;

    public ApiContactService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResponse<ContactDto>> GetAllAsync(ContactFilterParams filter)
    {
        var query = $"api/contacts?page={filter.Page}&perPage={filter.PerPage}";
        if (!string.IsNullOrEmpty(filter.Search)) query += $"&search={Uri.EscapeDataString(filter.Search)}";
        if (filter.CompanyId.HasValue) query += $"&companyId={filter.CompanyId}";
        if (filter.TagId.HasValue) query += $"&tagId={filter.TagId}";
        if (!string.IsNullOrEmpty(filter.Sort)) query += $"&sort={filter.Sort}";
        if (!string.IsNullOrEmpty(filter.Direction)) query += $"&direction={filter.Direction}";

        return await _http.GetFromJsonAsync<PaginatedResponse<ContactDto>>(query)
            ?? new PaginatedResponse<ContactDto>();
    }

    public async Task<ContactDetailDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ContactDetailDto>($"api/contacts/{id}");
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/contacts", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContactDto>()
            ?? throw new InvalidOperationException("Invalid create response.");
    }

    public async Task<ContactDto> UpdateAsync(int id, UpdateContactRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/contacts/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContactDto>()
            ?? throw new InvalidOperationException("Invalid update response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/contacts/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<ContactDto> SyncTagsAsync(int id, SyncTagsRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/contacts/{id}/tags", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContactDto>()
            ?? throw new InvalidOperationException("Invalid sync tags response.");
    }
}
