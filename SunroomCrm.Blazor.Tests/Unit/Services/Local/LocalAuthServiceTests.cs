using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Auth;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalAuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Name = "Test User",
            Email = "test@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await db.SaveChangesAsync();

        var service = new LocalAuthService(db, MockHttpContextAccessor.Create());
        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "test@test.com",
            Password = "password123"
        });

        result.User.Email.Should().Be("test@test.com");
        result.User.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Name = "Test User",
            Email = "test@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await db.SaveChangesAsync();

        var service = new LocalAuthService(db, MockHttpContextAccessor.Create());
        var act = () => service.LoginAsync(new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrong"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_NonexistentEmail_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAuthService(db, MockHttpContextAccessor.Create());

        var act = () => service.LoginAsync(new LoginRequest
        {
            Email = "nope@test.com",
            Password = "password123"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAuthService(db, MockHttpContextAccessor.Create());

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Name = "New User",
            Email = "new@test.com",
            Password = "password123"
        });

        result.User.Email.Should().Be("new@test.com");
        result.User.Name.Should().Be("New User");
        result.User.Id.Should().BeGreaterThan(0);
        db.Users.Should().ContainSingle(u => u.Email == "new@test.com");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Name = "Existing",
            Email = "existing@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await db.SaveChangesAsync();

        var service = new LocalAuthService(db, MockHttpContextAccessor.Create());
        var act = () => service.RegisterAsync(new RegisterRequest
        {
            Name = "New",
            Email = "existing@test.com",
            Password = "password123"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task GetCurrentUserAsync_AuthenticatedUser_ReturnsUserDto()
    {
        var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Id = 1,
            Name = "Admin",
            Email = "admin@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await db.SaveChangesAsync();

        var service = new LocalAuthService(db, MockHttpContextAccessor.Create(userId: 1));
        var result = await service.GetCurrentUserAsync();

        result.Id.Should().Be(1);
        result.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task GetCurrentUserAsync_NullHttpContext_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAuthService(db, MockHttpContextAccessor.CreateWithNullContext());

        var act = () => service.GetCurrentUserAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetCurrentUserAsync_NullUser_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalAuthService(db, MockHttpContextAccessor.CreateWithNullUser());

        var act = () => service.GetCurrentUserAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
