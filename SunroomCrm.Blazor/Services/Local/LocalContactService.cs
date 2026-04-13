using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Companies;
using SunroomCrm.Shared.DTOs.Contacts;
using SunroomCrm.Shared.DTOs.Deals;
using SunroomCrm.Shared.DTOs.Tags;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalContactService : IContactService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalContactService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginatedResponse<ContactDto>> GetAllAsync(ContactFilterParams filter)
    {
        var userId = GetCurrentUserId();
        var query = _db.Contacts
            .Include(c => c.Company)
            .Include(c => c.Tags)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)));
        }

        if (filter.CompanyId.HasValue)
            query = query.Where(c => c.CompanyId == filter.CompanyId);

        if (filter.TagId.HasValue)
            query = query.Where(c => c.Tags.Any(t => t.Id == filter.TagId));

        var total = await query.CountAsync();

        query = ApplySorting(query, filter.Sort, filter.Direction);
        var items = await query
            .Skip((filter.Page - 1) * filter.PerPage)
            .Take(filter.PerPage)
            .Select(c => new ContactDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Title = c.Title,
                CompanyName = c.Company != null ? c.Company.Name : null,
                CompanyId = c.CompanyId,
                LastContactedAt = c.LastContactedAt,
                Tags = c.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<ContactDto>
        {
            Data = items,
            Meta = new PaginationMeta
            {
                CurrentPage = filter.Page,
                PerPage = filter.PerPage,
                Total = total,
                LastPage = (int)Math.Ceiling(total / (double)filter.PerPage)
            }
        };
    }

    public async Task<ContactDetailDto?> GetByIdAsync(int id)
    {
        var contact = await _db.Contacts
            .Include(c => c.Company)
            .Include(c => c.Tags)
            .Include(c => c.Deals).ThenInclude(d => d.Company)
            .Include(c => c.Activities)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null) return null;

        return new ContactDetailDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Title = contact.Title,
            Notes = contact.Notes,
            LastContactedAt = contact.LastContactedAt,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt,
            Company = contact.Company != null ? new CompanyDto
            {
                Id = contact.Company.Id,
                Name = contact.Company.Name,
                Industry = contact.Company.Industry,
                Website = contact.Company.Website,
                Phone = contact.Company.Phone,
                City = contact.Company.City,
                State = contact.Company.State,
                CreatedAt = contact.Company.CreatedAt
            } : null,
            Tags = contact.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
            Deals = contact.Deals.Select(d => new DealDto
            {
                Id = d.Id,
                Title = d.Title,
                Value = d.Value,
                Stage = d.Stage.ToString(),
                ContactName = $"{contact.FirstName} {contact.LastName}",
                ContactId = contact.Id,
                CompanyName = d.Company?.Name,
                CompanyId = d.CompanyId,
                ExpectedCloseDate = d.ExpectedCloseDate,
                ClosedAt = d.ClosedAt,
                CreatedAt = d.CreatedAt
            }).ToList(),
            Activities = contact.Activities.OrderByDescending(a => a.OccurredAt).Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Subject = a.Subject,
                Body = a.Body,
                AiSummary = a.AiSummary,
                ContactId = a.ContactId,
                ContactName = $"{contact.FirstName} {contact.LastName}",
                DealId = a.DealId,
                OccurredAt = a.OccurredAt,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request)
    {
        var userId = GetCurrentUserId();
        var contact = new Contact
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Title = request.Title,
            Notes = request.Notes,
            CompanyId = request.CompanyId
        };

        if (request.TagIds?.Any() == true)
        {
            var tags = await _db.Tags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync();
            contact.Tags = tags;
        }

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();

        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Title = contact.Title,
            CompanyId = contact.CompanyId,
            Tags = contact.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
            CreatedAt = contact.CreatedAt
        };
    }

    public async Task<ContactDto> UpdateAsync(int id, UpdateContactRequest request)
    {
        var contact = await _db.Contacts
            .Include(c => c.Company)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Contact {id} not found.");

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.Title = request.Title;
        contact.Notes = request.Notes;
        contact.CompanyId = request.CompanyId;

        await _db.SaveChangesAsync();

        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Title = contact.Title,
            CompanyName = contact.Company?.Name,
            CompanyId = contact.CompanyId,
            Tags = contact.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
            CreatedAt = contact.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var contact = await _db.Contacts.FindAsync(id)
            ?? throw new KeyNotFoundException($"Contact {id} not found.");

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync();
    }

    public async Task<ContactDto> SyncTagsAsync(int id, SyncTagsRequest request)
    {
        var contact = await _db.Contacts
            .Include(c => c.Tags)
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Contact {id} not found.");

        contact.Tags.Clear();
        var tags = await _db.Tags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync();
        foreach (var tag in tags)
            contact.Tags.Add(tag);

        await _db.SaveChangesAsync();

        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Title = contact.Title,
            CompanyName = contact.Company?.Name,
            CompanyId = contact.CompanyId,
            Tags = contact.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
            CreatedAt = contact.CreatedAt
        };
    }

    private static IQueryable<Contact> ApplySorting(IQueryable<Contact> query, string? sort, string direction)
    {
        var isDesc = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort?.ToLower() switch
        {
            "firstname" => isDesc ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
            "lastname" => isDesc ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
            "email" => isDesc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "company" => isDesc ? query.OrderByDescending(c => c.Company!.Name) : query.OrderBy(c => c.Company!.Name),
            _ => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };
    }

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("Not authenticated.");
    }
}
