using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.AI;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Deals;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalDealService : IDealService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalDealService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginatedResponse<DealDto>> GetAllAsync(DealFilterParams filter)
    {
        var userId = GetCurrentUserId();
        var query = _db.Deals
            .Include(d => d.Contact)
            .Include(d => d.Company)
            .Where(d => d.UserId == userId);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(filter.Stage) && Enum.TryParse<DealStage>(filter.Stage, out var stage))
            query = query.Where(d => d.Stage == stage);

        var total = await query.CountAsync();

        var isDesc = filter.Direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        query = filter.Sort?.ToLower() switch
        {
            "title" => isDesc ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
            "value" => isDesc ? query.OrderByDescending(d => d.Value) : query.OrderBy(d => d.Value),
            "stage" => isDesc ? query.OrderByDescending(d => d.Stage) : query.OrderBy(d => d.Stage),
            _ => isDesc ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PerPage)
            .Take(filter.PerPage)
            .Select(d => new DealDto
            {
                Id = d.Id,
                Title = d.Title,
                Value = d.Value,
                Stage = d.Stage.ToString(),
                ContactName = d.Contact.FirstName + " " + d.Contact.LastName,
                ContactId = d.ContactId,
                CompanyName = d.Company != null ? d.Company.Name : null,
                CompanyId = d.CompanyId,
                ExpectedCloseDate = d.ExpectedCloseDate,
                ClosedAt = d.ClosedAt,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<DealDto>
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

    public async Task<DealDetailDto?> GetByIdAsync(int id)
    {
        var deal = await _db.Deals
            .Include(d => d.Contact)
            .Include(d => d.Company)
            .Include(d => d.Activities).ThenInclude(a => a.Contact)
            .Include(d => d.AiInsights)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deal == null) return null;

        return new DealDetailDto
        {
            Id = deal.Id,
            Title = deal.Title,
            Value = deal.Value,
            Stage = deal.Stage.ToString(),
            ContactName = $"{deal.Contact.FirstName} {deal.Contact.LastName}",
            ContactId = deal.ContactId,
            CompanyName = deal.Company?.Name,
            CompanyId = deal.CompanyId,
            ExpectedCloseDate = deal.ExpectedCloseDate,
            ClosedAt = deal.ClosedAt,
            Notes = deal.Notes,
            CreatedAt = deal.CreatedAt,
            UpdatedAt = deal.UpdatedAt,
            Activities = deal.Activities.OrderByDescending(a => a.OccurredAt).Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Subject = a.Subject,
                Body = a.Body,
                AiSummary = a.AiSummary,
                ContactId = a.ContactId,
                ContactName = a.Contact != null ? $"{a.Contact.FirstName} {a.Contact.LastName}" : null,
                DealId = a.DealId,
                OccurredAt = a.OccurredAt,
                CreatedAt = a.CreatedAt
            }).ToList(),
            Insights = deal.AiInsights.OrderByDescending(i => i.GeneratedAt).Select(i => new DealInsightDto
            {
                Id = i.Id,
                Insight = i.Insight,
                GeneratedAt = i.GeneratedAt
            }).ToList()
        };
    }

    public async Task<DealDto> CreateAsync(CreateDealRequest request)
    {
        var userId = GetCurrentUserId();
        var deal = new Deal
        {
            UserId = userId,
            ContactId = request.ContactId,
            CompanyId = request.CompanyId,
            Title = request.Title,
            Value = request.Value,
            Stage = Enum.TryParse<DealStage>(request.Stage, out var stage) ? stage : DealStage.Lead,
            ExpectedCloseDate = request.ExpectedCloseDate,
            Notes = request.Notes
        };

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        var contact = await _db.Contacts.FindAsync(deal.ContactId);
        var company = deal.CompanyId.HasValue ? await _db.Companies.FindAsync(deal.CompanyId) : null;

        return new DealDto
        {
            Id = deal.Id,
            Title = deal.Title,
            Value = deal.Value,
            Stage = deal.Stage.ToString(),
            ContactName = contact != null ? $"{contact.FirstName} {contact.LastName}" : string.Empty,
            ContactId = deal.ContactId,
            CompanyName = company?.Name,
            CompanyId = deal.CompanyId,
            ExpectedCloseDate = deal.ExpectedCloseDate,
            ClosedAt = deal.ClosedAt,
            CreatedAt = deal.CreatedAt
        };
    }

    public async Task<DealDto> UpdateAsync(int id, UpdateDealRequest request)
    {
        var deal = await _db.Deals
            .Include(d => d.Contact)
            .Include(d => d.Company)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Deal {id} not found.");

        deal.Title = request.Title;
        deal.Value = request.Value;
        deal.ContactId = request.ContactId;
        deal.CompanyId = request.CompanyId;
        if (request.Stage != null && Enum.TryParse<DealStage>(request.Stage, out var stage))
        {
            if (stage is DealStage.Won or DealStage.Lost && deal.ClosedAt == null)
                deal.ClosedAt = DateTime.UtcNow;
            deal.Stage = stage;
        }
        deal.ExpectedCloseDate = request.ExpectedCloseDate;
        deal.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return new DealDto
        {
            Id = deal.Id,
            Title = deal.Title,
            Value = deal.Value,
            Stage = deal.Stage.ToString(),
            ContactName = $"{deal.Contact.FirstName} {deal.Contact.LastName}",
            ContactId = deal.ContactId,
            CompanyName = deal.Company?.Name,
            CompanyId = deal.CompanyId,
            ExpectedCloseDate = deal.ExpectedCloseDate,
            ClosedAt = deal.ClosedAt,
            CreatedAt = deal.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var deal = await _db.Deals.FindAsync(id)
            ?? throw new KeyNotFoundException($"Deal {id} not found.");

        _db.Deals.Remove(deal);
        await _db.SaveChangesAsync();
    }

    public async Task<PipelineDto> GetPipelineAsync()
    {
        var userId = GetCurrentUserId();
        var deals = await _db.Deals
            .Include(d => d.Contact)
            .Include(d => d.Company)
            .Where(d => d.UserId == userId)
            .ToListAsync();

        var stages = Enum.GetValues<DealStage>()
            .Select(stage =>
            {
                var stageDeals = deals.Where(d => d.Stage == stage).ToList();
                return new PipelineStageDto
                {
                    Stage = stage.ToString(),
                    Count = stageDeals.Count,
                    TotalValue = stageDeals.Sum(d => d.Value),
                    Deals = stageDeals.Select(d => new DealDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Value = d.Value,
                        Stage = d.Stage.ToString(),
                        ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}",
                        ContactId = d.ContactId,
                        CompanyName = d.Company?.Name,
                        CompanyId = d.CompanyId,
                        ExpectedCloseDate = d.ExpectedCloseDate,
                        ClosedAt = d.ClosedAt,
                        CreatedAt = d.CreatedAt
                    }).ToList()
                };
            }).ToList();

        return new PipelineDto { Stages = stages };
    }

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("Not authenticated.");
    }
}
