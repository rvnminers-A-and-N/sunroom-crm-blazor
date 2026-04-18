using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

    public async IAsyncEnumerable<string> SummarizeStreamAsync(
        SummarizeRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/ai/summarize/stream")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _http.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line == "data: [DONE]") yield break;

            if (line.StartsWith("data: "))
            {
                var json = line[6..];
                var token = ParseToken(json);
                if (token != null)
                    yield return token;
            }
        }
    }

    private static string? ParseToken(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            return string.IsNullOrEmpty(token) ? null : token;
        }
        catch (JsonException) { return null; }
    }

    public async IAsyncEnumerable<string> SmartSearchStreamAsync(
        string query, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/ai/search/stream")
        {
            Content = JsonContent.Create(new { query })
        };

        using var response = await _http.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line == "data: [DONE]") yield break;

            if (line.StartsWith("data: "))
            {
                var json = line[6..];
                var token = ParseToken(json);
                if (token != null)
                    yield return token;
            }
        }
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

    public async IAsyncEnumerable<string> DealInsightsStreamAsync(
        int dealId, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/ai/deal-insights/{dealId}/stream")
        {
            Content = JsonContent.Create(new { })
        };

        using var response = await _http.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line == "data: [DONE]") yield break;

            if (line.StartsWith("data: "))
            {
                var json = line[6..];
                var token = ParseToken(json);
                if (token != null)
                    yield return token;
            }
        }
    }
}
