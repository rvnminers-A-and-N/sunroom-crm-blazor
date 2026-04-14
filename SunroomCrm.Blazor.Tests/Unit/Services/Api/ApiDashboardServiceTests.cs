using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Dashboard;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiDashboardServiceTests
{
    private static (ApiDashboardService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiDashboardService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsDashboard()
    {
        var (service, handler) = CreateService();
        var expected = new DashboardDto
        {
            TotalContacts = 50,
            TotalCompanies = 10,
            TotalDeals = 25,
            TotalPipelineValue = 500000m
        };
        handler.SetupResponse("/api/dashboard", expected);

        var result = await service.GetDashboardAsync();

        result.TotalContacts.Should().Be(50);
        result.TotalCompanies.Should().Be(10);
        result.TotalDeals.Should().Be(25);
        result.TotalPipelineValue.Should().Be(500000m);
    }
}
