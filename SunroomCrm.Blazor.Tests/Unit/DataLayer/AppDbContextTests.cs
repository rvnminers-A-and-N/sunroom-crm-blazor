using FluentAssertions;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.DataLayer;

public class AppDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_NewEntity_SetsCreatedAtAndUpdatedAt()
    {
        var db = TestDbContextFactory.Create();
        var user = new User { Name = "Test", Email = "test@test.com", Password = "hash" };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_UpdatesUpdatedAt()
    {
        var db = TestDbContextFactory.Create();
        var user = new User { Name = "Test", Email = "test@test.com", Password = "hash" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var originalCreatedAt = user.CreatedAt;

        user.Name = "Updated";
        await db.SaveChangesAsync();

        user.CreatedAt.Should().Be(originalCreatedAt);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SaveChanges_NewEntity_SetsTimestamps()
    {
        var db = TestDbContextFactory.Create();
        var tag = new Tag { Name = "Test Tag", Color = "#FF0000" };

        db.Tags.Add(tag);
        db.SaveChanges();

        tag.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DbContext_HasAllDbSets()
    {
        var db = TestDbContextFactory.Create();

        db.Users.Should().NotBeNull();
        db.Companies.Should().NotBeNull();
        db.Contacts.Should().NotBeNull();
        db.Tags.Should().NotBeNull();
        db.Deals.Should().NotBeNull();
        db.Activities.Should().NotBeNull();
        db.AiInsights.Should().NotBeNull();
    }
}
