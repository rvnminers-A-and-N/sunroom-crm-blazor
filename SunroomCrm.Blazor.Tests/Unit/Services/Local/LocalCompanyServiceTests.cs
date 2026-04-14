using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Common;
using SunroomCrm.Shared.DTOs.Companies;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalCompanyServiceTests
{
    private static (LocalCompanyService service, Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        var accessor = MockHttpContextAccessor.Create(userId: 1);

        db.Users.Add(new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" });
        db.Companies.AddRange(
            new Company { Id = 1, UserId = 1, Name = "Acme Corp", Industry = "Tech" },
            new Company { Id = 2, UserId = 1, Name = "Beta Inc", Industry = "Finance" },
            new Company { Id = 3, UserId = 2, Name = "Other Co", Industry = "Other" }
        );
        db.SaveChanges();

        return (new LocalCompanyService(db, accessor), db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUserCompanies()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(null, new PaginationParams());

        result.Data.Should().HaveCount(2);
        result.Meta.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync("acme", new PaginationParams());

        result.Data.Should().ContainSingle().Which.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCompany_ReturnsDetail()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Acme Corp");
        result.Industry.Should().Be("Tech");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesCompany()
    {
        var (service, db) = CreateService();
        var result = await service.CreateAsync(new CreateCompanyRequest
        {
            Name = "New Corp",
            Industry = "Healthcare"
        });

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Corp");
        result.ContactCount.Should().Be(0);
        db.Companies.Should().HaveCount(4);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesCompany()
    {
        var (service, _) = CreateService();
        var result = await service.UpdateAsync(1, new UpdateCompanyRequest
        {
            Name = "Acme Updated",
            Industry = "Biotech"
        });

        result.Name.Should().Be("Acme Updated");
        result.Industry.Should().Be("Biotech");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.UpdateAsync(999, new UpdateCompanyRequest { Name = "X" });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingCompany_RemovesFromDb()
    {
        var (service, db) = CreateService();
        await service.DeleteAsync(1);

        db.Companies.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.DeleteAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
