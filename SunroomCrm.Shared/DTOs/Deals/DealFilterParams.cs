using SunroomCrm.Shared.DTOs.Common;

namespace SunroomCrm.Shared.DTOs.Deals;

public class DealFilterParams : PaginationParams
{
    public string? Search { get; set; }
    public string? Stage { get; set; }
    public int? UserId { get; set; }
}
