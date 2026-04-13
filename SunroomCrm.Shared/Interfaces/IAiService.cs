using SunroomCrm.Shared.DTOs.AI;

namespace SunroomCrm.Shared.Interfaces;

public interface IAiService
{
    Task<SummarizeResponse> SummarizeAsync(SummarizeRequest request);
    Task<SmartSearchResponse> SmartSearchAsync(SmartSearchRequest request);
    Task<List<DealInsightDto>> GetDealInsightsAsync(int dealId);
    Task<DealInsightDto> GenerateDealInsightAsync(int dealId);
}
