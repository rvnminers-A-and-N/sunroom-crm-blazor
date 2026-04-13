using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Companies;
using SunroomCrm.Shared.DTOs.Contacts;
using SunroomCrm.Shared.DTOs.Deals;
using SunroomCrm.Shared.DTOs.Tags;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalCompanyService : ICompanyService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalCompanyService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginatedResponse<CompanyDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var userId = GetCurrentUserId();
        var query = _db.Companies
            .Include(c => c.Contacts)
            .Include(c => c.Deals)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) ||
                (c.Industry != null && c.Industry.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();

        var isDesc = pagination.Direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        query = pagination.Sort?.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "industry" => isDesc ? query.OrderByDescending(c => c.Industry) : query.OrderBy(c => c.Industry),
            _ => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PerPage)
            .Take(pagination.PerPage)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Industry = c.Industry,
                Website = c.Website,
                Phone = c.Phone,
                City = c.City,
                State = c.State,
                ContactCount = c.Contacts.Count,
                DealCount = c.Deals.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<CompanyDto>
        {
            Data = items,
            Meta = new PaginationMeta
            {
                CurrentPage = pagination.Page,
                PerPage = pagination.PerPage,
                Total = total,
                LastPage = (int)Math.Ceiling(total / (double)pagination.PerPage)
            }
        };
    }

    public async Task<CompanyDetailDto?> GetByIdAsync(int id)
    {
        var company = await _db.Companies
            .Include(c => c.Contacts).ThenInclude(c => c.Tags)
            .Include(c => c.Deals).ThenInclude(d => d.Contact)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null) return null;

        return new CompanyDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            Industry = company.Industry,
            Website = company.Website,
            Phone = company.Phone,
            Address = company.Address,
            City = company.City,
            State = company.State,
            Zip = company.Zip,
            Notes = company.Notes,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            Contacts = company.Contacts.Select(c => new ContactDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Title = c.Title,
                CompanyName = company.Name,
                CompanyId = company.Id,
                LastContactedAt = c.LastContactedAt,
                Tags = c.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
                CreatedAt = c.CreatedAt
            }).ToList(),
            Deals = company.Deals.Select(d => new DealDto
            {
                Id = d.Id,
                Title = d.Title,
                Value = d.Value,
                Stage = d.Stage.ToString(),
                ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}",
                ContactId = d.ContactId,
                CompanyName = company.Name,
                CompanyId = company.Id,
                ExpectedCloseDate = d.ExpectedCloseDate,
                ClosedAt = d.ClosedAt,
                CreatedAt = d.CreatedAt
            }).ToList()
        };
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request)
    {
        var userId = GetCurrentUserId();
        var company = new Company
        {
            UserId = userId,
            Name = request.Name,
            Industry = request.Industry,
            Website = request.Website,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Zip = request.Zip,
            Notes = request.Notes
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Industry = company.Industry,
            Website = company.Website,
            Phone = company.Phone,
            City = company.City,
            State = company.State,
            ContactCount = 0,
            DealCount = 0,
            CreatedAt = company.CreatedAt
        };
    }

    public async Task<CompanyDto> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        var company = await _db.Companies
            .Include(c => c.Contacts)
            .Include(c => c.Deals)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Company {id} not found.");

        company.Name = request.Name;
        company.Industry = request.Industry;
        company.Website = request.Website;
        company.Phone = request.Phone;
        company.Address = request.Address;
        company.City = request.City;
        company.State = request.State;
        company.Zip = request.Zip;
        company.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Industry = company.Industry,
            Website = company.Website,
            Phone = company.Phone,
            City = company.City,
            State = company.State,
            ContactCount = company.Contacts.Count,
            DealCount = company.Deals.Count,
            CreatedAt = company.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var company = await _db.Companies.FindAsync(id)
            ?? throw new KeyNotFoundException($"Company {id} not found.");

        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
    }

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("Not authenticated.");
    }
}
