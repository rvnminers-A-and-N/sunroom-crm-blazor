using System.Net;
using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Tags;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiTagServiceTests
{
    private static (ApiTagService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiTagService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTags()
    {
        var (service, handler) = CreateService();
        var expected = new List<TagDto>
        {
            new() { Id = 1, Name = "VIP", Color = "#02795F" }
        };
        handler.SetupResponse("/api/tags", expected);

        var result = await service.GetAllAsync();

        result.Should().ContainSingle().Which.Name.Should().Be("VIP");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTag_ReturnsDto()
    {
        var (service, handler) = CreateService();
        var expected = new TagDto { Id = 1, Name = "VIP", Color = "#02795F" };
        handler.SetupResponse("/api/tags/1", expected);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("VIP");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedTag()
    {
        var (service, handler) = CreateService();
        var expected = new TagDto { Id = 1, Name = "New Tag", Color = "#FF0000" };
        handler.SetupResponse("/api/tags", expected);

        var result = await service.CreateAsync(new CreateTagRequest
        {
            Name = "New Tag",
            Color = "#FF0000"
        });

        result.Name.Should().Be("New Tag");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DoesNotThrow()
    {
        var (service, handler) = CreateService();
        handler.SetupResponse("/api/tags/1", HttpStatusCode.NoContent);

        var act = () => service.DeleteAsync(1);

        await act.Should().NotThrowAsync();
    }
}
