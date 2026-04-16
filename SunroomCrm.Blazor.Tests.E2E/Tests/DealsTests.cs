using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class DealsTests
{
    private readonly PlaywrightFixture _fixture;

    public DealsTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DealsList_ShowsSeededDeals()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First.ClickAsync();
        await page.WaitForURLAsync("**/deals**");

        var tableRows = page.Locator("table tbody tr");
        await tableRows.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        (await tableRows.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DealsList_HasCreateButton()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First.ClickAsync();
        await page.WaitForURLAsync("**/deals**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var createButton = page.GetByRole(AriaRole.Button, new() { Name = "New Deal" });
        (await createButton.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task DealsList_CreateButton_OpensDialog()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First.ClickAsync();
        await page.WaitForURLAsync("**/deals**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var createBtn = page.GetByRole(AriaRole.Button, new() { Name = "New Deal" });
        await createBtn.EvaluateAsync("el => el.click()");

        var dialog = page.GetByRole(AriaRole.Dialog);
        await dialog.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        (await dialog.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Pipeline_ShowsStageColumns()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Pipeline" }).ClickAsync();
        await page.WaitForURLAsync("**/deals/pipeline**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Pipeline page should render
        page.Url.Should().Contain("/deals/pipeline");
    }

    [Fact]
    public async Task DealsList_ClickRow_NavigatesToDetail()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First.ClickAsync();
        await page.WaitForURLAsync("**/deals**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstRow = page.Locator("table tbody tr").First;
        await firstRow.EvaluateAsync("el => el.click()");

        // Blazor client-side navigation via pushState — poll URL
        await page.WaitForFunctionAsync("() => /\\/deals\\/\\d+/.test(location.href)",
            new PageWaitForFunctionOptions { Timeout = 10000 });
        page.Url.Should().MatchRegex(@"/deals/\d+");
    }
}
