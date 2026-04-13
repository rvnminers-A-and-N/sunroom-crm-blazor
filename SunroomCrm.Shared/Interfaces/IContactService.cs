using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Contacts;

namespace SunroomCrm.Shared.Interfaces;

public interface IContactService
{
    Task<PaginatedResponse<ContactDto>> GetAllAsync(ContactFilterParams filter);
    Task<ContactDetailDto?> GetByIdAsync(int id);
    Task<ContactDto> CreateAsync(CreateContactRequest request);
    Task<ContactDto> UpdateAsync(int id, UpdateContactRequest request);
    Task DeleteAsync(int id);
    Task<ContactDto> SyncTagsAsync(int id, SyncTagsRequest request);
}
