using FluentAssertions;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Blazor.Tests.Helpers;

namespace SunroomCrm.Blazor.Tests.Integration;

public class SeedDataTests
{
    [Fact]
    public async Task SeedAsync_SeedsAllEntities()
    {
        var db = TestDbContextFactory.Create();

        await SeedData.SeedAsync(db);

        db.Users.Should().HaveCount(3);
        db.Tags.Should().HaveCount(6);
        db.Companies.Should().HaveCount(5);
        db.Contacts.Should().HaveCountGreaterThan(0);
        db.Deals.Should().HaveCountGreaterThan(0);
        db.Activities.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        var db = TestDbContextFactory.Create();

        await SeedData.SeedAsync(db);
        await SeedData.SeedAsync(db);

        db.Users.Should().HaveCount(3);
    }

    [Fact]
    public async Task SeedAsync_AdminUserHasHashedPassword()
    {
        var db = TestDbContextFactory.Create();

        await SeedData.SeedAsync(db);

        var admin = db.Users.First(u => u.Email == "admin@sunroomcrm.com");
        BCrypt.Net.BCrypt.Verify("password123", admin.Password).Should().BeTrue();
    }
}
