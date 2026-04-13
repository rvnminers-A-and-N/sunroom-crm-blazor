using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.AI;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiAiService : IAiService
{
    private readonly HttpClient _http;

    public ApiAiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SummarizeResponse> SummarizeAsync(SummarizeRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/ai/summarize", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SummarizeResponse>()
            ?? new SummarizeResponse();
    }

    public async Task<SmartSearchResponse> SmartSearchAsync(SmartSearchRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/ai/search", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SmartSearchResponse>()
            ?? new SmartSearchResponse();
    }

    public async Task<List<DealInsightDto>> GetDealInsightsAsync(int dealId)
    {
        return await _http.GetFromJsonAsync<List<DealInsightDto>>($"api/deals/{dealId}/insights")
            ?? new List<DealInsightDto>();
    }

    public async Task<DealInsightDto> GenerateDealInsightAsync(int dealId)
    {
        var response = await _http.PostAsync($"api/deals/{dealId}/insights", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DealInsightDto>()
            ?? throw new InvalidOperationException("Invalid insight response.");
    }
}
