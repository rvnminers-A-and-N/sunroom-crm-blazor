using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Auth;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiAuthServiceTests
{
    private static (ApiAuthService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiAuthService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var (service, handler) = CreateService();
        var expected = new AuthResponse
        {
            Token = "jwt-token",
            User = new UserDto { Id = 1, Name = "Admin", Email = "admin@test.com", Role = "Admin" }
        };
        handler.SetupResponse("/api/auth/login", expected);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@test.com",
            Password = "password123"
        });

        result.Token.Should().Be("jwt-token");
        result.User.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        var (service, handler) = CreateService();
        var expected = new AuthResponse
        {
            Token = "jwt-token",
            User = new UserDto { Id = 2, Name = "New User", Email = "new@test.com", Role = "User" }
        };
        handler.SetupResponse("/api/auth/register", expected);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Name = "New User",
            Email = "new@test.com",
            Password = "password123"
        });

        result.User.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsUser()
    {
        var (service, handler) = CreateService();
        var expected = new UserDto { Id = 1, Name = "Admin", Email = "admin@test.com", Role = "Admin" };
        handler.SetupResponse("/api/auth/me", expected);

        var result = await service.GetCurrentUserAsync();

        result.Email.Should().Be("admin@test.com");
    }
}
