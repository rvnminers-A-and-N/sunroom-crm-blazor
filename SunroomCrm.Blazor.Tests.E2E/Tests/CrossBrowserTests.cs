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

        // Page renders without JS errors
        consoleErrors.Should().BeEmpty(
            "login page should render without console errors on {0}", browserType);

        // Key elements are visible
        var signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
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

        // Dashboard renders without JS errors
        consoleErrors.Should().BeEmpty(
            "dashboard should render without console errors on {0}", browserType);
    }
}
