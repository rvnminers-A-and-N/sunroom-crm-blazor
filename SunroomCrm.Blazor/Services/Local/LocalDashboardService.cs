using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Dashboard;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalDashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalDashboardService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var userId = GetCurrentUserId();

        var totalContacts = await _db.Contacts.CountAsync(c => c.UserId == userId);
        var totalCompanies = await _db.Companies.CountAsync(c => c.UserId == userId);

        var deals = await _db.Deals
            .Where(d => d.UserId == userId)
            .ToListAsync();

        var totalDeals = deals.Count;
        var totalPipelineValue = deals
            .Where(d => d.Stage != DealStage.Won && d.Stage != DealStage.Lost)
            .Sum(d => d.Value);
        var wonRevenue = deals
            .Where(d => d.Stage == DealStage.Won)
            .Sum(d => d.Value);

        var dealsByStage = Enum.GetValues<DealStage>()
            .Select(stage =>
            {
                var stageDeals = deals.Where(d => d.Stage == stage).ToList();
                return new DealStageCount
                {
                    Stage = stage.ToString(),
                    Count = stageDeals.Count,
                    TotalValue = stageDeals.Sum(d => d.Value)
                };
            })
            .ToList();

        var recentActivities = await _db.Activities
            .Include(a => a.User)
            .Include(a => a.Contact)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(10)
            .Select(a => new RecentActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Subject = a.Subject,
                ContactName = a.Contact != null ? a.Contact.FirstName + " " + a.Contact.LastName : null,
                UserName = a.User.Name,
                OccurredAt = a.OccurredAt
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalContacts = totalContacts,
            TotalCompanies = totalCompanies,
            TotalDeals = totalDeals,
            TotalPipelineValue = totalPipelineValue,
            WonRevenue = wonRevenue,
            DealsByStage = dealsByStage,
            RecentActivities = recentActivities
        };
    }

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("Not authenticated.");
    }
}
