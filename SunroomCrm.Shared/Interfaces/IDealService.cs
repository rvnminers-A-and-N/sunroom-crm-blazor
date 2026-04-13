using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Deals;

namespace SunroomCrm.Shared.Interfaces;

public interface IDealService
{
    Task<PaginatedResponse<DealDto>> GetAllAsync(DealFilterParams filter);
    Task<DealDetailDto?> GetByIdAsync(int id);
    Task<DealDto> CreateAsync(CreateDealRequest request);
    Task<DealDto> UpdateAsync(int id, UpdateDealRequest request);
    Task DeleteAsync(int id);
    Task<PipelineDto> GetPipelineAsync();
}
