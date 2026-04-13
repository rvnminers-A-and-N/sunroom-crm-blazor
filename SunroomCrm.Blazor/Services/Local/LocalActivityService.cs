using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalActivityService : IActivityService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalActivityService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginatedResponse<ActivityDto>> GetAllAsync(ActivityFilterParams filter)
    {
        var userId = GetCurrentUserId();
        var query = _db.Activities
            .Include(a => a.User)
            .Include(a => a.Contact)
            .Include(a => a.Deal)
            .Where(a => a.UserId == userId);

        if (filter.ContactId.HasValue)
            query = query.Where(a => a.ContactId == filter.ContactId);

        if (filter.DealId.HasValue)
            query = query.Where(a => a.DealId == filter.DealId);

        if (!string.IsNullOrEmpty(filter.Type) && Enum.TryParse<ActivityType>(filter.Type, out var type))
            query = query.Where(a => a.Type == type);

        var total = await query.CountAsync();

        var isDesc = filter.Direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        query = filter.Sort?.ToLower() switch
        {
            "subject" => isDesc ? query.OrderByDescending(a => a.Subject) : query.OrderBy(a => a.Subject),
            "type" => isDesc ? query.OrderByDescending(a => a.Type) : query.OrderBy(a => a.Type),
            _ => isDesc ? query.OrderByDescending(a => a.OccurredAt) : query.OrderBy(a => a.OccurredAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PerPage)
            .Take(filter.PerPage)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Subject = a.Subject,
                Body = a.Body,
                AiSummary = a.AiSummary,
                ContactId = a.ContactId,
                ContactName = a.Contact != null ? a.Contact.FirstName + " " + a.Contact.LastName : null,
                DealId = a.DealId,
                DealTitle = a.Deal != null ? a.Deal.Title : null,
                UserName = a.User.Name,
                OccurredAt = a.OccurredAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<ActivityDto>
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

    public async Task<ActivityDto?> GetByIdAsync(int id)
    {
        var activity = await _db.Activities
            .Include(a => a.User)
            .Include(a => a.Contact)
            .Include(a => a.Deal)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null) return null;

        return new ActivityDto
        {
            Id = activity.Id,
            Type = activity.Type.ToString(),
            Subject = activity.Subject,
            Body = activity.Body,
            AiSummary = activity.AiSummary,
            ContactId = activity.ContactId,
            ContactName = activity.Contact != null ? $"{activity.Contact.FirstName} {activity.Contact.LastName}" : null,
            DealId = activity.DealId,
            DealTitle = activity.Deal?.Title,
            UserName = activity.User.Name,
            OccurredAt = activity.OccurredAt,
            CreatedAt = activity.CreatedAt
        };
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityRequest request)
    {
        var userId = GetCurrentUserId();
        var activity = new Activity
        {
            UserId = userId,
            ContactId = request.ContactId,
            DealId = request.DealId,
            Type = Enum.TryParse<ActivityType>(request.Type, out var type) ? type : ActivityType.Note,
            Subject = request.Subject,
            Body = request.Body,
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);

        return new ActivityDto
        {
            Id = activity.Id,
            Type = activity.Type.ToString(),
            Subject = activity.Subject,
            Body = activity.Body,
            ContactId = activity.ContactId,
            DealId = activity.DealId,
            UserName = user?.Name ?? string.Empty,
            OccurredAt = activity.OccurredAt,
            CreatedAt = activity.CreatedAt
        };
    }

    public async Task<ActivityDto> UpdateAsync(int id, UpdateActivityRequest request)
    {
        var activity = await _db.Activities
            .Include(a => a.User)
            .Include(a => a.Contact)
            .Include(a => a.Deal)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Activity {id} not found.");

        activity.Type = Enum.TryParse<ActivityType>(request.Type, out var type) ? type : activity.Type;
        activity.Subject = request.Subject;
        activity.Body = request.Body;
        activity.ContactId = request.ContactId;
        activity.DealId = request.DealId;
        if (request.OccurredAt.HasValue)
            activity.OccurredAt = request.OccurredAt.Value;

        await _db.SaveChangesAsync();

        return new ActivityDto
        {
            Id = activity.Id,
            Type = activity.Type.ToString(),
            Subject = activity.Subject,
            Body = activity.Body,
            AiSummary = activity.AiSummary,
            ContactId = activity.ContactId,
            ContactName = activity.Contact != null ? $"{activity.Contact.FirstName} {activity.Contact.LastName}" : null,
            DealId = activity.DealId,
            DealTitle = activity.Deal?.Title,
            UserName = activity.User.Name,
            OccurredAt = activity.OccurredAt,
            CreatedAt = activity.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var activity = await _db.Activities.FindAsync(id)
            ?? throw new KeyNotFoundException($"Activity {id} not found.");

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
    }

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("Not authenticated.");
    }
}
