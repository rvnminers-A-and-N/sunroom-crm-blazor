using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.AI;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Contacts;
using SunroomCrm.Shared.DTOs.Tags;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalAiService : IAiService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalAiService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<SummarizeResponse> SummarizeAsync(SummarizeRequest request)
    {
        // Stub implementation — returns a generated summary
        var words = request.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var summary = words.Length > 20
            ? string.Join(' ', words.Take(20)) + "..."
            : request.Text;

        return Task.FromResult(new SummarizeResponse { Summary = $"Summary: {summary}" });
    }

    public async IAsyncEnumerable<string> SummarizeStreamAsync(
        SummarizeRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var result = await SummarizeAsync(request);
        foreach (var word in result.Summary.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(20, ct);
        }
    }

    public async IAsyncEnumerable<string> SmartSearchStreamAsync(
        string query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var placeholder = $"Searching for '{query}' across your contacts, deals, and activities. Found several relevant matches based on your query.";
        foreach (var word in placeholder.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(20, ct);
        }
    }

    public async Task<SmartSearchResponse> SmartSearchAsync(SmartSearchRequest request)
    {
        var userId = GetCurrentUserId();
        var query = request.Query.ToLower();

        var contacts = await _db.Contacts
            .Include(c => c.Tags)
            .Where(c => c.UserId == userId &&
                (c.FirstName.ToLower().Contains(query) ||
                 c.LastName.ToLower().Contains(query) ||
                 (c.Email != null && c.Email.ToLower().Contains(query)) ||
                 (c.Notes != null && c.Notes.ToLower().Contains(query))))
            .Take(10)
            .Select(c => new ContactDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Title = c.Title,
                CompanyId = c.CompanyId,
                Tags = c.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color, CreatedAt = t.CreatedAt }).ToList(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        var activities = await _db.Activities
            .Include(a => a.Contact)
            .Where(a => a.UserId == userId &&
                (a.Subject.ToLower().Contains(query) ||
                 (a.Body != null && a.Body.ToLower().Contains(query))))
            .Take(10)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Subject = a.Subject,
                Body = a.Body,
                ContactId = a.ContactId,
                ContactName = a.Contact != null ? a.Contact.FirstName + " " + a.Contact.LastName : null,
                DealId = a.DealId,
                OccurredAt = a.OccurredAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new SmartSearchResponse
        {
            Interpretation = $"Searching for '{request.Query}' across contacts and activities.",
            Contacts = contacts,
            Activities = activities
        };
    }

    public async Task<List<DealInsightDto>> GetDealInsightsAsync(int dealId)
    {
        return await _db.AiInsights
            .Where(i => i.DealId == dealId)
            .OrderByDescending(i => i.GeneratedAt)
            .Select(i => new DealInsightDto
            {
                Id = i.Id,
                Insight = i.Insight,
                GeneratedAt = i.GeneratedAt
            })
            .ToListAsync();
    }

    public async Task<DealInsightDto> GenerateDealInsightAsync(int dealId)
    {
        var deal = await _db.Deals
            .Include(d => d.Contact)
            .Include(d => d.Activities)
            .FirstOrDefaultAsync(d => d.Id == dealId)
            ?? throw new KeyNotFoundException($"Deal {dealId} not found.");

        // Stub insight generation
        var activityCount = deal.Activities.Count;
        var insight = new AiInsight
        {
            DealId = dealId,
            Insight = $"Deal '{deal.Title}' is in {deal.Stage} stage valued at ${deal.Value:N0}. " +
                      $"Contact: {deal.Contact.FirstName} {deal.Contact.LastName}. " +
                      $"{activityCount} activities logged. " +
                      "Recommendation: Continue engagement and schedule follow-up."
        };

        _db.AiInsights.Add(insight);
        await _db.SaveChangesAsync();

        return new DealInsightDto
        {
            Id = insight.Id,
            Insight = insight.Insight,
            GeneratedAt = insight.GeneratedAt
        };
    }

    public async IAsyncEnumerable<string> DealInsightsStreamAsync(
        int dealId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var placeholder = $"Analyzing deal {dealId}. This deal shows strong engagement with multiple touchpoints. Recommendation: continue nurturing the relationship and schedule a follow-up.";
        foreach (var word in placeholder.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(20, ct);
        }
    }

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException("Not authenticated.");
    }
}
