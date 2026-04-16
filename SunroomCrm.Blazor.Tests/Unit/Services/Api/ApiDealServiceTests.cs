using System.Net;
using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Deals;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiDealServiceTests
{
    private static (ApiDealService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiDealService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDeals()
    {
        var (service, handler) = CreateService();
        var expected = new PaginatedResponse<DealDto>
        {
            Data = new List<DealDto>
            {
                new() { Id = 1, Title = "Big Deal", Value = 50000 }
            },
            Meta = new PaginationMeta { CurrentPage = 1, PerPage = 25, Total = 1, LastPage = 1 }
        };
        handler.SetupResponse("/api/deals", expected);

        var result = await service.GetAllAsync(new DealFilterParams());

        result.Data.Should().ContainSingle().Which.Title.Should().Be("Big Deal");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDeal_ReturnsDetail()
    {
        var (service, handler) = CreateService();
        var expected = new DealDetailDto { Id = 1, Title = "Big Deal", Value = 50000, Stage = "Proposal" };
        handler.SetupResponse("/api/deals/1", expected);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Big Deal");
        result.Stage.Should().Be("Proposal");
    }

    [Fact]
    public async Task GetPipelineAsync_ReturnsPipeline()
    {
        var (service, handler) = CreateService();
        var expected = new PipelineDto
        {
            Stages = new List<PipelineStageDto>
            {
                new() { Stage = "Lead", Count = 3, TotalValue = 30000, Deals = new List<DealDto>() }
            }
        };
        handler.SetupResponse("/api/deals/pipeline", expected);

        var result = await service.GetPipelineAsync();

        result.Stages.Should().ContainSingle().Which.Stage.Should().Be("Lead");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DoesNotThrow()
    {
        var (service, handler) = CreateService();
        handler.SetupResponse("/api/deals/1", HttpStatusCode.NoContent);

        var act = () => service.DeleteAsync(1);

        await act.Should().NotThrowAsync();
    }
}
