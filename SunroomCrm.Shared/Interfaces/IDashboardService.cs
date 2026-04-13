using SunroomCrm.Shared.DTOs.Dashboard;

namespace SunroomCrm.Shared.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
