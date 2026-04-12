using System.ComponentModel.DataAnnotations;

namespace SunroomCrm.Shared.DTOs.Contacts;

public class SyncTagsRequest
{
    [Required]
    public List<int> TagIds { get; set; } = new();
}
