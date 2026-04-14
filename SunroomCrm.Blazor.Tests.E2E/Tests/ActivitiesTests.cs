using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class ActivitiesTests
{
    private readonly PlaywrightFixture _fixture;

    public ActivitiesTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ActivitiesList_ShowsSeededActivities()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Activities" }).ClickAsync();
        await page.WaitForURLAsync("**/activities**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableRows = page.Locator("table tbody tr");
        (await tableRows.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ActivitiesList_HasCreateButton()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Activities" }).ClickAsync();
        await page.WaitForURLAsync("**/activities**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var createButton = page.GetByRole(AriaRole.Button, new() { Name = "New Activity" });
        (await createButton.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task ActivitiesList_CreateButton_OpensDialog()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Activities" }).ClickAsync();
        await page.WaitForURLAsync("**/activities**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByRole(AriaRole.Button, new() { Name = "New Activity" }).ClickAsync();

        var dialog = page.GetByRole(AriaRole.Dialog);
        (await dialog.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task ActivitiesList_HasTypeFilter()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Activities" }).ClickAsync();
        await page.WaitForURLAsync("**/activities**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Activity list should have filter controls
        var filters = page.Locator(".mud-select");
        (await filters.CountAsync()).Should().BeGreaterThan(0);
    }
}
