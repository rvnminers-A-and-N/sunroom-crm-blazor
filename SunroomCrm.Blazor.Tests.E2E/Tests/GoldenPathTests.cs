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

        // 1. Register a new user via API call (bypasses Blazor circuit timing)
        await page.GotoAsync($"{_fixture.BaseUrl}/register");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var registerOk = await page.EvaluateAsync<bool>(@"
            async (credentials) => {
                const r = await fetch('/api/account/register', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(credentials)
                });
                return r.ok;
            }
        ", new { name = "Golden Path User", email = uniqueEmail, password = "Test123!" });

        registerOk.Should().BeTrue("register API should accept new user");

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        page.Url.Should().Contain("/dashboard");

        // 2. Navigate to contacts
        var contactsLink = page.GetByRole(AriaRole.Link, new() { Name = "Contacts" });
        await contactsLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await contactsLink.ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        page.Url.Should().Contain("/contacts");

        // 3. Navigate to deals
        var dealsLink = page.GetByRole(AriaRole.Link, new() { Name = "Deals" }).First;
        await dealsLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await dealsLink.ClickAsync();
        await page.WaitForURLAsync("**/deals**");
        page.Url.Should().Contain("/deals");

        // 4. Navigate to pipeline (use Exact to avoid matching "Pipeline View" button)
        await page.GetByRole(AriaRole.Link, new() { Name = "Pipeline", Exact = true }).ClickAsync();
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
        await tableRows.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        (await tableRows.CountAsync()).Should().BeGreaterThan(0);
    }
}
