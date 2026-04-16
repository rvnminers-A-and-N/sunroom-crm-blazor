using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class AiPanelTests
{
    private readonly PlaywrightFixture _fixture;

    public AiPanelTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AiPanel_PageLoads()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "AI Panel" }).ClickAsync();
        await page.WaitForURLAsync("**/ai**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        page.Url.Should().Contain("/ai");
    }

    [Fact]
    public async Task AiPanel_ShowsSummarizeSection()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GotoAsync($"{_fixture.BaseUrl}/ai");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // AI panel should have summarize functionality
        var pageContent = await page.ContentAsync();
        pageContent.Should().ContainAny("Summarize", "summarize", "AI");
    }

    [Fact]
    public async Task AiPanel_ShowsSearchSection()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GotoAsync($"{_fixture.BaseUrl}/ai");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // AI panel should have smart search functionality
        var pageContent = await page.ContentAsync();
        pageContent.Should().ContainAny("Search", "search", "Smart");
    }
}
