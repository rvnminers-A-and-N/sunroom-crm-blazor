using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;

namespace SunroomCrm.Blazor.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static AppDbContext CreateWithSeed(string? dbName = null)
    {
        var context = Create(dbName);
        SeedData.SeedAsync(context).GetAwaiter().GetResult();
        return context;
    }
}
