using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.AI;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalAiServiceTests
{
    [Fact]
    public async Task SummarizeAsync_ShortText_ReturnsFullText()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());

        var result = await service.SummarizeAsync(new SummarizeRequest { Text = "Short text" });

        result.Summary.Should().Be("Summary: Short text");
    }

    [Fact]
    public async Task SummarizeAsync_LongText_ReturnsTruncatedSummary()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());

        var longText = string.Join(' ', Enumerable.Range(1, 30).Select(i => $"word{i}"));
        var result = await service.SummarizeAsync(new SummarizeRequest { Text = longText });

        result.Summary.Should().StartWith("Summary: ");
        result.Summary.Should().EndWith("...");
    }

    [Fact]
    public async Task SmartSearchAsync_FindsContactsByName()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@test.com", Password = "hash" });
        db.Contacts.Add(new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            UserId = 1
        });
        await db.SaveChangesAsync();

        var service = new LocalAiService(db, MockHttpContextAccessor.Create(userId: 1));
        var result = await service.SmartSearchAsync(new SmartSearchRequest { Query = "John" });

        result.Contacts.Should().ContainSingle().Which.FirstName.Should().Be("John");
        result.Interpretation.Should().Contain("John");
    }

    [Fact]
    public async Task SmartSearchAsync_FindsActivitiesBySubject()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@test.com", Password = "hash" });
        db.Activities.Add(new Activity
        {
            Id = 1,
            Subject = "Follow-up meeting",
            Type = ActivityType.Meeting,
            UserId = 1,
            OccurredAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new LocalAiService(db, MockHttpContextAccessor.Create(userId: 1));
        var result = await service.SmartSearchAsync(new SmartSearchRequest { Query = "follow-up" });

        result.Activities.Should().ContainSingle().Which.Subject.Should().Contain("Follow-up");
    }

    [Fact]
    public async Task GetDealInsightsAsync_ReturnsInsightsForDeal()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@test.com", Password = "hash" });
        db.Contacts.Add(new Contact { Id = 1, FirstName = "John", LastName = "Doe", UserId = 1 });
        db.Deals.Add(new Deal { Id = 1, Title = "Big Deal", Value = 50000, Stage = DealStage.Proposal, ContactId = 1, UserId = 1 });
        db.AiInsights.Add(new AiInsight { Id = 1, DealId = 1, Insight = "Looking good." });
        await db.SaveChangesAsync();

        var service = new LocalAiService(db, MockHttpContextAccessor.Create(userId: 1));
        var result = await service.GetDealInsightsAsync(1);

        result.Should().ContainSingle().Which.Insight.Should().Be("Looking good.");
    }

    [Fact]
    public async Task GenerateDealInsightAsync_CreatesNewInsight()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@test.com", Password = "hash" });
        db.Contacts.Add(new Contact { Id = 1, FirstName = "Jane", LastName = "Smith", UserId = 1 });
        db.Deals.Add(new Deal { Id = 1, Title = "Enterprise Deal", Value = 100000, Stage = DealStage.Negotiation, ContactId = 1, UserId = 1 });
        await db.SaveChangesAsync();

        var service = new LocalAiService(db, MockHttpContextAccessor.Create(userId: 1));
        var result = await service.GenerateDealInsightAsync(1);

        result.Insight.Should().Contain("Enterprise Deal");
        result.Insight.Should().Contain("Jane Smith");
        db.AiInsights.Should().ContainSingle();
    }

    [Fact]
    public async Task GenerateDealInsightAsync_NonexistentDeal_ThrowsKeyNotFound()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());

        var act = () => service.GenerateDealInsightAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*999*");
    }

    [Fact]
    public async Task SmartSearchAsync_NullHttpContext_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.CreateWithNullContext());

        var act = () => service.SmartSearchAsync(new SmartSearchRequest { Query = "test" });
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SmartSearchAsync_NullUser_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.CreateWithNullUser());

        var act = () => service.SmartSearchAsync(new SmartSearchRequest { Query = "test" });
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SummarizeStreamAsync_YieldsWords()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());

        var tokens = new List<string>();
        await foreach (var token in service.SummarizeStreamAsync(new SummarizeRequest { Text = "Short text" }))
        {
            tokens.Add(token);
        }

        var combined = string.Join("", tokens).Trim();
        combined.Should().Be("Summary: Short text");
    }

    [Fact]
    public async Task SmartSearchStreamAsync_YieldsWords()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());

        var tokens = new List<string>();
        await foreach (var token in service.SmartSearchStreamAsync("test"))
        {
            tokens.Add(token);
        }

        var combined = string.Join("", tokens).Trim();
        combined.Should().Contain("test");
    }

    [Fact]
    public async Task DealInsightsStreamAsync_YieldsWords()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());

        var tokens = new List<string>();
        await foreach (var token in service.DealInsightsStreamAsync(1))
        {
            tokens.Add(token);
        }

        var combined = string.Join("", tokens).Trim();
        combined.Should().Contain("deal 1");
    }

    [Fact]
    public async Task SummarizeStreamAsync_RespectsCancellation()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAiService(db, MockHttpContextAccessor.Create());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () =>
        {
            await foreach (var _ in service.SummarizeStreamAsync(
                new SummarizeRequest { Text = "test" }, cts.Token))
            { }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
