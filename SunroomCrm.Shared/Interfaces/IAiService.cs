using SunroomCrm.Shared.DTOs.AI;

namespace SunroomCrm.Shared.Interfaces;

public interface IAiService
{
    Task<SummarizeResponse> SummarizeAsync(SummarizeRequest request);
    IAsyncEnumerable<string> SummarizeStreamAsync(SummarizeRequest request, CancellationToken ct = default);
    Task<SmartSearchResponse> SmartSearchAsync(SmartSearchRequest request);
    IAsyncEnumerable<string> SmartSearchStreamAsync(string query, CancellationToken ct = default);
    Task<List<DealInsightDto>> GetDealInsightsAsync(int dealId);
    Task<DealInsightDto> GenerateDealInsightAsync(int dealId);
    IAsyncEnumerable<string> DealInsightsStreamAsync(int dealId, CancellationToken ct = default);
}
