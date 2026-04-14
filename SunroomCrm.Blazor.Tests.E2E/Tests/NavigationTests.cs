using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class NavigationTests
{
    private readonly PlaywrightFixture _fixture;

    public NavigationTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Sidebar_DashboardLink_NavigatesToDashboard()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" }).ClickAsync();
        await page.WaitForURLAsync("**/dashboard**");

        page.Url.Should().Contain("/dashboard");
    }

    [Fact]
    public async Task Sidebar_ContactsLink_NavigatesToContacts()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");

        page.Url.Should().Contain("/contacts");
    }

    [Fact]
    public async Task Sidebar_CompaniesLink_NavigatesToCompanies()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Companies" }).ClickAsync();
        await page.WaitForURLAsync("**/companies**");

        page.Url.Should().Contain("/companies");
    }

    [Fact]
    public async Task Sidebar_DealsLink_NavigatesToDeals()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First.ClickAsync();
        await page.WaitForURLAsync("**/deals**");

        page.Url.Should().Contain("/deals");
    }

    [Fact]
    public async Task Sidebar_ActivitiesLink_NavigatesToActivities()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Activities" }).ClickAsync();
        await page.WaitForURLAsync("**/activities**");

        page.Url.Should().Contain("/activities");
    }

    [Fact]
    public async Task Sidebar_ShowsBrandLogo()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        var brandText = page.Locator(".brand-text");
        (await brandText.IsVisibleAsync()).Should().BeTrue();
        (await brandText.TextContentAsync()).Should().Contain("Sunroom CRM");
    }
}
