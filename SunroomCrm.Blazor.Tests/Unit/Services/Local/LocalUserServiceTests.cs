using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalUserServiceTests
{
    private static (LocalUserService service, Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        db.Users.AddRange(
            new User { Id = 1, Name = "Admin", Email = "admin@test.com", Password = BCrypt.Net.BCrypt.HashPassword("pass") },
            new User { Id = 2, Name = "User", Email = "user@test.com", Password = BCrypt.Net.BCrypt.HashPassword("pass") }
        );
        db.SaveChanges();

        return (new LocalUserService(db), db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDto()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
        result.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesUser()
    {
        var (service, _) = CreateService();
        var result = await service.UpdateAsync(1, new UpdateUserRequest
        {
            Name = "Updated Admin",
            Email = "updated@test.com"
        });

        result.Name.Should().Be("Updated Admin");
        result.Email.Should().Be("updated@test.com");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        var (service, _) = CreateService();
        var act = () => service.UpdateAsync(1, new UpdateUserRequest
        {
            Email = "user@test.com"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task UpdateAsync_WithPassword_HashesPassword()
    {
        var (service, db) = CreateService();
        await service.UpdateAsync(1, new UpdateUserRequest
        {
            Password = "newpass123"
        });

        var user = await db.Users.FindAsync(1);
        BCrypt.Net.BCrypt.Verify("newpass123", user!.Password).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.UpdateAsync(999, new UpdateUserRequest { Name = "X" });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_RemovesFromDb()
    {
        var (service, db) = CreateService();
        await service.DeleteAsync(2);

        db.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.DeleteAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
