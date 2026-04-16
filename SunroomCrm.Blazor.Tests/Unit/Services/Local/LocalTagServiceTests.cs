using FluentAssertions;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Blazor.Tests.Helpers;
using SunroomCrm.Shared.DTOs.Tags;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Local;

public class LocalTagServiceTests
{
    private static (LocalTagService service, Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        db.Tags.AddRange(
            new Tag { Id = 1, Name = "VIP", Color = "#FF0000" },
            new Tag { Id = 2, Name = "Lead", Color = "#00FF00" },
            new Tag { Id = 3, Name = "Hot", Color = "#FF9900" }
        );
        db.SaveChanges();

        return (new LocalTagService(db), db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTags()
    {
        var (service, _) = CreateService();
        var result = await service.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTag_ReturnsTag()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("VIP");
        result.Color.Should().Be("#FF0000");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesTag()
    {
        var (service, db) = CreateService();
        var result = await service.CreateAsync(new CreateTagRequest
        {
            Name = "New Tag",
            Color = "#0000FF"
        });

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Tag");
        db.Tags.Should().HaveCount(4);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesTag()
    {
        var (service, _) = CreateService();
        var result = await service.UpdateAsync(1, new UpdateTagRequest
        {
            Name = "Updated VIP",
            Color = "#FF00FF"
        });

        result.Name.Should().Be("Updated VIP");
        result.Color.Should().Be("#FF00FF");
    }

    [Fact]
    public async Task DeleteAsync_ExistingTag_RemovesFromDb()
    {
        var (service, db) = CreateService();
        await service.DeleteAsync(1);

        db.Tags.Should().HaveCount(2);
    }
}
