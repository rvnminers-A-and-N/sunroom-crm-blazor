using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Contacts;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalContactServiceTests
{
    private static (LocalContactService service, Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        var accessor = MockHttpContextAccessor.Create(userId: 1);

        db.Users.Add(new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" });
        db.Tags.AddRange(
            new Tag { Id = 1, Name = "VIP", Color = "#FF0000" },
            new Tag { Id = 2, Name = "Lead", Color = "#00FF00" }
        );
        db.Companies.Add(new Company { Id = 1, UserId = 1, Name = "Acme Corp" });
        db.Contacts.AddRange(
            new Contact { Id = 1, UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", CompanyId = 1 },
            new Contact { Id = 2, UserId = 1, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" },
            new Contact { Id = 3, UserId = 2, FirstName = "Other", LastName = "User", Email = "other@test.com" }
        );
        db.SaveChanges();

        return (new LocalContactService(db, accessor), db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUserContacts()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ContactFilterParams());

        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(c => c.Id == 1 || c.Id == 2);
        result.Meta.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ContactFilterParams { Search = "john" });

        result.Data.Should().ContainSingle()
            .Which.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetAllAsync_WithCompanyFilter_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ContactFilterParams { CompanyId = 1 });

        result.Data.Should().ContainSingle()
            .Which.CompanyName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ContactFilterParams { Page = 1, PerPage = 1 });

        result.Data.Should().HaveCount(1);
        result.Meta.Total.Should().Be(2);
        result.Meta.LastPage.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingContact_ReturnsDetail()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.Company.Should().NotBeNull();
        result.Company!.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesContact()
    {
        var (service, db) = CreateService();
        var result = await service.CreateAsync(new CreateContactRequest
        {
            FirstName = "New",
            LastName = "Contact",
            Email = "new@test.com"
        });

        result.Id.Should().BeGreaterThan(0);
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("Contact");
        db.Contacts.Should().HaveCount(4);
    }

    [Fact]
    public async Task CreateAsync_WithTags_AssociatesTags()
    {
        var (service, _) = CreateService();
        var result = await service.CreateAsync(new CreateContactRequest
        {
            FirstName = "Tagged",
            LastName = "Contact",
            TagIds = new List<int> { 1, 2 }
        });

        result.Tags.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesContact()
    {
        var (service, _) = CreateService();
        var result = await service.UpdateAsync(1, new UpdateContactRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@test.com"
        });

        result.FirstName.Should().Be("Updated");
        result.Email.Should().Be("updated@test.com");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.UpdateAsync(999, new UpdateContactRequest
        {
            FirstName = "X",
            LastName = "Y"
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingContact_RemovesFromDb()
    {
        var (service, db) = CreateService();
        await service.DeleteAsync(1);

        db.Contacts.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.DeleteAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SyncTagsAsync_ValidRequest_ReplacesAllTags()
    {
        var (service, db) = CreateService();
        var contact = db.Contacts.First(c => c.Id == 1);
        contact.Tags.Add(db.Tags.First(t => t.Id == 1));
        await db.SaveChangesAsync();

        var result = await service.SyncTagsAsync(1, new SyncTagsRequest
        {
            TagIds = new List<int> { 2 }
        });

        result.Tags.Should().ContainSingle().Which.Name.Should().Be("Lead");
    }

    [Fact]
    public async Task GetAllAsync_WithTagFilter_FiltersResults()
    {
        var (service, db) = CreateService();
        var contact = db.Contacts.First(c => c.Id == 1);
        contact.Tags.Add(db.Tags.First(t => t.Id == 1));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync(new ContactFilterParams { TagId = 1 });

        result.Data.Should().ContainSingle().Which.FirstName.Should().Be("John");
    }

    [Theory]
    [InlineData("firstname", "asc")]
    [InlineData("firstname", "desc")]
    [InlineData("lastname", "asc")]
    [InlineData("lastname", "desc")]
    [InlineData("email", "asc")]
    [InlineData("email", "desc")]
    [InlineData("company", "asc")]
    [InlineData("company", "desc")]
    [InlineData(null, "asc")]
    [InlineData(null, "desc")]
    public async Task GetAllAsync_WithSorting_ReturnsResults(string? sort, string direction)
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new ContactFilterParams
        {
            Sort = sort,
            Direction = direction
        });

        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task SyncTagsAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.SyncTagsAsync(999, new SyncTagsRequest { TagIds = new List<int> { 1 } });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_NullHttpContext_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalContactService(db, MockHttpContextAccessor.CreateWithNullContext());

        var act = () => service.GetAllAsync(new ContactFilterParams());
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetAllAsync_NullUser_ThrowsUnauthorized()
    {
        var db = TestDbContextFactory.Create();
        var service = new LocalContactService(db, MockHttpContextAccessor.CreateWithNullUser());

        var act = () => service.GetAllAsync(new ContactFilterParams());
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
