using System.ComponentModel.DataAnnotations;

namespace SunroomCrm.Shared.DTOs.AI;

public class SmartSearchRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;
}
