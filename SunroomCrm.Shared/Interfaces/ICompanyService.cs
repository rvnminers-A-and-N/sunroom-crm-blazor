using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Companies;

namespace SunroomCrm.Shared.Interfaces;

public interface ICompanyService
{
    Task<PaginatedResponse<CompanyDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<CompanyDetailDto?> GetByIdAsync(int id);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request);
    Task<CompanyDto> UpdateAsync(int id, UpdateCompanyRequest request);
    Task DeleteAsync(int id);
}
