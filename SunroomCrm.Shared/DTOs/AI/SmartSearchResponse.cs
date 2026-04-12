using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Contacts;

namespace SunroomCrm.Shared.DTOs.AI;

public class SmartSearchResponse
{
    public string Interpretation { get; set; } = string.Empty;
    public List<ContactDto> Contacts { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
}
