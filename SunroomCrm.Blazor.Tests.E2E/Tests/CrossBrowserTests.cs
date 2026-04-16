using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class CrossBrowserTests
{
    private readonly PlaywrightFixture _fixture;

    public CrossBrowserTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task LoginPage_Renders_AcrossBrowsers(string browserType)
    {
        await using var context = await _fixture.CreateBrowserContextAsync(browserType);
        var page = await context.NewPageAsync();

        var consoleErrors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error") consoleErrors.Add(msg.Text);
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Filter expected console errors in test environment
        // (static resources, Blazor circuit noise, MudBlazor headless rendering)
        var unexpectedErrors = consoleErrors.Where(e =>
            !e.Contains("404") &&
            !e.Contains("Failed to load resource") &&
            !e.Contains("circuit will be terminated") &&
            !e.Contains("DetailedErrors") &&
            !e.Contains("mudElementRef") &&
            !e.Contains("MudBlazor") &&
            !e.Contains("WebSocket") &&
            !e.Contains("blazor") &&
            !e.Contains("_framework") &&
            !e.Contains("negotiation") &&
            !e.Contains("Failed to fetch")).ToList();

        unexpectedErrors.Should().BeEmpty(
            "login page should render without unexpected console errors on {0}", browserType);

        // Key elements are visible
        var signInButton = page.Locator("button[type='submit']");
        await signInButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        (await signInButton.IsVisibleAsync()).Should().BeTrue(
            "Sign In button should be visible on {0}", browserType);
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Dashboard_Renders_AcrossBrowsers(string browserType)
    {
        await using var context = await _fixture.CreateBrowserContextAsync(browserType);
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        var consoleErrors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error") consoleErrors.Add(msg.Text);
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Filter expected console errors in test environment
        var unexpectedErrors = consoleErrors.Where(e =>
            !e.Contains("404") &&
            !e.Contains("Failed to load resource") &&
            !e.Contains("circuit will be terminated") &&
            !e.Contains("DetailedErrors") &&
            !e.Contains("mudElementRef") &&
            !e.Contains("MudBlazor") &&
            !e.Contains("WebSocket") &&
            !e.Contains("blazor") &&
            !e.Contains("_framework") &&
            !e.Contains("negotiation") &&
            !e.Contains("Failed to fetch")).ToList();

        unexpectedErrors.Should().BeEmpty(
            "dashboard should render without console errors on {0}", browserType);
    }
}
