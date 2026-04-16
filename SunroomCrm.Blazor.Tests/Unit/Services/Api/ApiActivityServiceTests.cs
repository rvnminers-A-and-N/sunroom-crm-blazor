using System.Net;
using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Activities;
using SunroomCrm.Shared.DTOs.Common;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiActivityServiceTests
{
    private static (ApiActivityService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiActivityService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsActivities()
    {
        var (service, handler) = CreateService();
        var expected = new PaginatedResponse<ActivityDto>
        {
            Data = new List<ActivityDto>
            {
                new() { Id = 1, Subject = "Follow-up call", Type = "Call" }
            },
            Meta = new PaginationMeta { CurrentPage = 1, PerPage = 25, Total = 1, LastPage = 1 }
        };
        handler.SetupResponse("/api/activities", expected);

        var result = await service.GetAllAsync(new ActivityFilterParams());

        result.Data.Should().ContainSingle().Which.Subject.Should().Be("Follow-up call");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingActivity_ReturnsDto()
    {
        var (service, handler) = CreateService();
        var expected = new ActivityDto { Id = 1, Subject = "Meeting", Type = "Meeting" };
        handler.SetupResponse("/api/activities/1", expected);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Meeting");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedActivity()
    {
        var (service, handler) = CreateService();
        var expected = new ActivityDto { Id = 1, Subject = "New Call", Type = "Call" };
        handler.SetupResponse("/api/activities", expected);

        var result = await service.CreateAsync(new CreateActivityRequest
        {
            Subject = "New Call",
            Type = "Call"
        });

        result.Subject.Should().Be("New Call");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DoesNotThrow()
    {
        var (service, handler) = CreateService();
        handler.SetupResponse("/api/activities/1", HttpStatusCode.NoContent);

        var act = () => service.DeleteAsync(1);

        await act.Should().NotThrowAsync();
    }
}
