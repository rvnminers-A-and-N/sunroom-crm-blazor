using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class GoldenPathTests
{
    private readonly PlaywrightFixture _fixture;

    public GoldenPathTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FullJourney_Register_Dashboard_Contacts_Deals()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        var uniqueEmail = $"golden.{Guid.NewGuid():N}@test.sunroom.dev";

        // 1. Register a new user
        await page.GotoAsync($"{_fixture.BaseUrl}/register");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByLabel("Full Name").FillAsync("Golden Path User");
        await page.GetByLabel("Email").FillAsync(uniqueEmail);
        await page.GetByLabel("Password", new() { Exact = true }).FillAsync("Test123!");
        await page.GetByLabel("Confirm Password").FillAsync("Test123!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

        await page.WaitForURLAsync("**/dashboard**", new() { Timeout = 10000 });
        page.Url.Should().Contain("/dashboard");

        // 2. Navigate to contacts
        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        page.Url.Should().Contain("/contacts");

        // 3. Navigate to deals
        await page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First.ClickAsync();
        await page.WaitForURLAsync("**/deals**");
        page.Url.Should().Contain("/deals");

        // 4. Navigate to pipeline
        await page.GetByRole(AriaRole.Link, new() { Name = "Pipeline" }).ClickAsync();
        await page.WaitForURLAsync("**/deals/pipeline**");
        page.Url.Should().Contain("/deals/pipeline");
    }

    [Fact]
    public async Task Dashboard_ShowsStatCards()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var statCards = page.Locator(".stat-card");
        (await statCards.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_ShowsGradientBanner()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var banner = page.Locator(".gradient-banner");
        (await banner.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Contacts_SeededDataVisible()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Seeded data should show contacts in the table
        var tableRows = page.Locator("table tbody tr");
        (await tableRows.CountAsync()).Should().BeGreaterThan(0);
    }
}
