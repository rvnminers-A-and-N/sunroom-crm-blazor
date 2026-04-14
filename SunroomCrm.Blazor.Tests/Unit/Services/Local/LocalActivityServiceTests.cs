using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalActivityServiceTests
{
    private static (LocalActivityService service, Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        var accessor = MockHttpContextAccessor.Create(userId: 1);

        db.Users.Add(new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" });
        db.Contacts.Add(new Contact { Id = 1, UserId = 1, FirstName = "John", LastName = "Doe" });
        db.Deals.Add(new Deal { Id = 1, UserId = 1, ContactId = 1, Title = "Test Deal", Value = 1000, Stage = DealStage.Lead });
        db.Activities.AddRange(
            new Activity { Id = 1, UserId = 1, ContactId = 1, Type = ActivityType.Note, Subject = "Meeting note", OccurredAt = DateTime.UtcNow },
            new Activity { Id = 2, UserId = 1, ContactId = 1, DealId = 1, Type = ActivityType.Call, Subject = "Follow-up call", OccurredAt = DateTime.UtcNow.AddHours(-1) },
            new Activity { Id = 3, UserId = 2, ContactId = 1, Type = ActivityType.Email, Subject = "Other user", OccurredAt = DateTime.UtcNow }
        );
        db.SaveChanges();

        return (new LocalActivityService(db, accessor), db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUserActivities()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ActivityFilterParams());

        result.Data.Should().HaveCount(2);
        result.Meta.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithTypeFilter_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ActivityFilterParams { Type = "Call" });

        result.Data.Should().ContainSingle().Which.Subject.Should().Be("Follow-up call");
    }

    [Fact]
    public async Task GetAllAsync_WithContactFilter_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ActivityFilterParams { ContactId = 1 });

        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingActivity_ReturnsDto()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Meeting note");
        result.Type.Should().Be("Note");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesActivity()
    {
        var (service, db) = CreateService();
        var result = await service.CreateAsync(new CreateActivityRequest
        {
            ContactId = 1,
            Type = "Meeting",
            Subject = "New meeting"
        });

        result.Id.Should().BeGreaterThan(0);
        result.Subject.Should().Be("New meeting");
        result.Type.Should().Be("Meeting");
        db.Activities.Should().HaveCount(4);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesActivity()
    {
        var (service, _) = CreateService();
        var result = await service.UpdateAsync(1, new UpdateActivityRequest
        {
            ContactId = 1,
            Type = "Email",
            Subject = "Updated subject"
        });

        result.Subject.Should().Be("Updated subject");
        result.Type.Should().Be("Email");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.UpdateAsync(999, new UpdateActivityRequest
        {
            Subject = "X",
            Type = "Note"
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingActivity_RemovesFromDb()
    {
        var (service, db) = CreateService();
        await service.DeleteAsync(1);

        db.Activities.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.DeleteAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
