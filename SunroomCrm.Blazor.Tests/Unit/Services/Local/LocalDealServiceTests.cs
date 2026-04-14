using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Deals;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalDealServiceTests
{
    private static (LocalDealService service, Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        var accessor = MockHttpContextAccessor.Create(userId: 1);

        db.Users.Add(new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" });
        var contact = new Contact { Id = 1, UserId = 1, FirstName = "John", LastName = "Doe" };
        db.Contacts.Add(contact);
        db.Companies.Add(new Company { Id = 1, UserId = 1, Name = "Acme Corp" });
        db.Deals.AddRange(
            new Deal { Id = 1, UserId = 1, ContactId = 1, Title = "Big Deal", Value = 50000, Stage = DealStage.Proposal, CompanyId = 1 },
            new Deal { Id = 2, UserId = 1, ContactId = 1, Title = "Small Deal", Value = 5000, Stage = DealStage.Lead },
            new Deal { Id = 3, UserId = 1, ContactId = 1, Title = "Won Deal", Value = 30000, Stage = DealStage.Won },
            new Deal { Id = 4, UserId = 2, ContactId = 1, Title = "Other Deal", Value = 10000, Stage = DealStage.Lead }
        );
        db.SaveChanges();

        return (new LocalDealService(db, accessor), db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUserDeals()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new DealFilterParams());

        result.Data.Should().HaveCount(3);
        result.Meta.Total.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new DealFilterParams { Search = "big" });

        result.Data.Should().ContainSingle().Which.Title.Should().Be("Big Deal");
    }

    [Fact]
    public async Task GetAllAsync_WithStageFilter_FiltersResults()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync(new DealFilterParams { Stage = "Lead" });

        result.Data.Should().ContainSingle().Which.Title.Should().Be("Small Deal");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDeal_ReturnsDetail()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Big Deal");
        result.Value.Should().Be(50000);
        result.Stage.Should().Be("Proposal");
        result.CompanyName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesDeal()
    {
        var (service, db) = CreateService();
        var result = await service.CreateAsync(new CreateDealRequest
        {
            Title = "New Deal",
            Value = 25000,
            ContactId = 1,
            Stage = "Qualified"
        });

        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("New Deal");
        result.Value.Should().Be(25000);
        result.Stage.Should().Be("Qualified");
        db.Deals.Should().HaveCount(5);
    }

    [Fact]
    public async Task UpdateAsync_StageToWon_SetsClosedAt()
    {
        var (service, db) = CreateService();
        var result = await service.UpdateAsync(1, new UpdateDealRequest
        {
            Title = "Big Deal",
            Value = 50000,
            ContactId = 1,
            Stage = "Won"
        });

        result.Stage.Should().Be("Won");
        result.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ThrowsKeyNotFound()
    {
        var (service, _) = CreateService();
        var act = () => service.UpdateAsync(999, new UpdateDealRequest
        {
            Title = "X",
            Value = 0,
            ContactId = 1
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingDeal_RemovesFromDb()
    {
        var (service, db) = CreateService();
        await service.DeleteAsync(1);

        db.Deals.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPipelineAsync_ReturnsAllStages()
    {
        var (service, _) = CreateService();
        var result = await service.GetPipelineAsync();

        result.Stages.Should().HaveCount(6);
        result.Stages.Should().Contain(s => s.Stage == "Proposal" && s.Count == 1);
        result.Stages.Should().Contain(s => s.Stage == "Won" && s.Count == 1);
        result.Stages.Should().Contain(s => s.Stage == "Lead" && s.Count == 1);
    }

    [Fact]
    public async Task GetPipelineAsync_CalculatesTotalValues()
    {
        var (service, _) = CreateService();
        var result = await service.GetPipelineAsync();

        var proposalStage = result.Stages.First(s => s.Stage == "Proposal");
        proposalStage.TotalValue.Should().Be(50000);
    }
}
