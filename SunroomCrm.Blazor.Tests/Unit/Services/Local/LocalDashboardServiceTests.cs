using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalDashboardServiceTests
{
    private static LocalDashboardService CreateService()
    {
        var db = TestDbContextFactory.Create();
        var accessor = MockHttpContextAccessor.Create(userId: 1);

        db.Users.Add(new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" });
        var contact = new Contact { Id = 1, UserId = 1, FirstName = "John", LastName = "Doe" };
        db.Contacts.Add(contact);
        db.Companies.AddRange(
            new Company { Id = 1, UserId = 1, Name = "Acme" },
            new Company { Id = 2, UserId = 1, Name = "Beta" }
        );
        db.Deals.AddRange(
            new Deal { Id = 1, UserId = 1, ContactId = 1, Title = "Open Deal", Value = 10000, Stage = DealStage.Proposal },
            new Deal { Id = 2, UserId = 1, ContactId = 1, Title = "Won Deal", Value = 25000, Stage = DealStage.Won }
        );
        db.Activities.Add(new Activity
        {
            Id = 1,
            UserId = 1,
            ContactId = 1,
            Type = ActivityType.Note,
            Subject = "Test note",
            OccurredAt = DateTime.UtcNow
        });
        db.SaveChanges();

        return new LocalDashboardService(db, accessor);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsCounts()
    {
        var service = CreateService();
        var result = await service.GetDashboardAsync();

        result.TotalContacts.Should().Be(1);
        result.TotalCompanies.Should().Be(2);
        result.TotalDeals.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardAsync_CalculatesPipelineValue()
    {
        var service = CreateService();
        var result = await service.GetDashboardAsync();

        result.TotalPipelineValue.Should().Be(10000);
    }

    [Fact]
    public async Task GetDashboardAsync_CalculatesWonRevenue()
    {
        var service = CreateService();
        var result = await service.GetDashboardAsync();

        result.WonRevenue.Should().Be(25000);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsDealsByStage()
    {
        var service = CreateService();
        var result = await service.GetDashboardAsync();

        result.DealsByStage.Should().HaveCount(6);
        result.DealsByStage.Should().Contain(s => s.Stage == "Proposal" && s.Count == 1);
        result.DealsByStage.Should().Contain(s => s.Stage == "Won" && s.Count == 1);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsRecentActivities()
    {
        var service = CreateService();
        var result = await service.GetDashboardAsync();

        result.RecentActivities.Should().ContainSingle()
            .Which.Subject.Should().Be("Test note");
    }

    [Fact]
    public async Task GetDashboardAsync_NullHttpContext_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalDashboardService(db, MockHttpContextAccessor.CreateWithNullContext());

        var act = () => service.GetDashboardAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetDashboardAsync_NullUser_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalDashboardService(db, MockHttpContextAccessor.CreateWithNullUser());

        var act = () => service.GetDashboardAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
