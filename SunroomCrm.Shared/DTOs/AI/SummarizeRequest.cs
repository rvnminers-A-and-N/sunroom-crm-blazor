using System.ComponentModel.DataAnnotations;

namespace SunroomCrm.Shared.DTOs.AI;

public class SummarizeRequest
{
    [Required]
    public string Text { get; set; } = string.Empty;
}
