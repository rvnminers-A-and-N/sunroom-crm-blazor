using SunroomCrm.Shared.DTOs.Common;

namespace SunroomCrm.Shared.DTOs.Activities;

public class ActivityFilterParams : PaginationParams
{
    public int? ContactId { get; set; }
    public int? DealId { get; set; }
    public string? Type { get; set; }
}
