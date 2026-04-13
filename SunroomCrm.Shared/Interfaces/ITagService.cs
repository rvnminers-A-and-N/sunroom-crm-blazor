using SunroomCrm.Shared.DTOs.Tags;

namespace SunroomCrm.Shared.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(int id);
    Task<TagDto> CreateAsync(CreateTagRequest request);
    Task<TagDto> UpdateAsync(int id, UpdateTagRequest request);
    Task DeleteAsync(int id);
}
