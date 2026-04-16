using System.Net;
using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Companies;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiCompanyServiceTests
{
    private static (ApiCompanyService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiCompanyService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCompanies()
    {
        var (service, handler) = CreateService();
        var expected = new PaginatedResponse<CompanyDto>
        {
            Data = new List<CompanyDto>
            {
                new() { Id = 1, Name = "Acme Corp", Industry = "Tech" }
            },
            Meta = new PaginationMeta { CurrentPage = 1, PerPage = 25, Total = 1, LastPage = 1 }
        };
        handler.SetupResponse("/api/companies", expected);

        var result = await service.GetAllAsync(null, new PaginationParams());

        result.Data.Should().ContainSingle().Which.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCompany_ReturnsDetail()
    {
        var (service, handler) = CreateService();
        var expected = new CompanyDetailDto { Id = 1, Name = "Acme Corp", Industry = "Tech" };
        handler.SetupResponse("/api/companies/1", expected);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedCompany()
    {
        var (service, handler) = CreateService();
        var expected = new CompanyDto { Id = 1, Name = "New Co" };
        handler.SetupResponse("/api/companies", expected);

        var result = await service.CreateAsync(new CreateCompanyRequest { Name = "New Co" });

        result.Name.Should().Be("New Co");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DoesNotThrow()
    {
        var (service, handler) = CreateService();
        handler.SetupResponse("/api/companies/1", HttpStatusCode.NoContent);

        var act = () => service.DeleteAsync(1);

        await act.Should().NotThrowAsync();
    }
}
