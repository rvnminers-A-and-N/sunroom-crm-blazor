using System.Net;
using FluentAssertions;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Contacts;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class ApiContactServiceTests
{
    private static (ApiContactService service, MockHttpMessageHandler handler) CreateService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        var service = new ApiContactService(httpClient);
        return (service, handler);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsContacts()
    {
        var (service, handler) = CreateService();
        var expected = new PaginatedResponse<ContactDto>
        {
            Data = new List<ContactDto>
            {
                new() { Id = 1, FirstName = "John", LastName = "Doe" }
            },
            Meta = new PaginationMeta { CurrentPage = 1, PerPage = 25, Total = 1, LastPage = 1 }
        };
        handler.SetupResponse("/api/contacts", expected);

        var result = await service.GetAllAsync(new ContactFilterParams());

        result.Data.Should().ContainSingle().Which.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingContact_ReturnsDetail()
    {
        var (service, handler) = CreateService();
        var expected = new ContactDetailDto { Id = 1, FirstName = "John", LastName = "Doe" };
        handler.SetupResponse("/api/contacts/1", expected);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedContact()
    {
        var (service, handler) = CreateService();
        var expected = new ContactDto { Id = 1, FirstName = "New", LastName = "Contact" };
        handler.SetupResponse("/api/contacts", expected);

        var result = await service.CreateAsync(new CreateContactRequest
        {
            FirstName = "New",
            LastName = "Contact"
        });

        result.FirstName.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DoesNotThrow()
    {
        var (service, handler) = CreateService();
        handler.SetupResponse("/api/contacts/1", HttpStatusCode.NoContent);

        var act = () => service.DeleteAsync(1);

        await act.Should().NotThrowAsync();
    }
}
