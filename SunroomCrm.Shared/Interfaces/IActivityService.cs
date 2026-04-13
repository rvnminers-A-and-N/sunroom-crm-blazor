using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Common;

namespace SunroomCrm.Shared.Interfaces;

public interface IActivityService
{
    Task<PaginatedResponse<ActivityDto>> GetAllAsync(ActivityFilterParams filter);
    Task<ActivityDto?> GetByIdAsync(int id);
    Task<ActivityDto> CreateAsync(CreateActivityRequest request);
    Task<ActivityDto> UpdateAsync(int id, UpdateActivityRequest request);
    Task DeleteAsync(int id);
}
