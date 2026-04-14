using System.Net;
using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Auth;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiUserServiceTests
{
    private static (ApiUserService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiUserService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsers()
    {
        var (service, handler) = CreateService();
        var expected = new List<UserDto>
        {
            new() { Id = 1, Name = "Admin", Email = "admin@test.com", Role = "Admin" }
        };
        handler.SetupResponse("/api/users", expected);

        var result = await service.GetAllAsync();

        result.Should().ContainSingle().Which.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDto()
    {
        var (service, handler) = CreateService();
        var expected = new UserDto { Id = 1, Name = "Admin", Email = "admin@test.com", Role = "Admin" };
        handler.SetupResponse("/api/users/1", expected);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedUser()
    {
        var (service, handler) = CreateService();
        var expected = new UserDto { Id = 1, Name = "Updated", Email = "admin@test.com", Role = "Admin" };
        handler.SetupResponse("/api/users/1", expected);

        var result = await service.UpdateAsync(1, new UpdateUserRequest { Name = "Updated" });

        result.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DoesNotThrow()
    {
        var (service, handler) = CreateService();
        handler.SetupResponse("/api/users/1", HttpStatusCode.NoContent);

        var act = () => service.DeleteAsync(1);

        await act.Should().NotThrowAsync();
    }
}
