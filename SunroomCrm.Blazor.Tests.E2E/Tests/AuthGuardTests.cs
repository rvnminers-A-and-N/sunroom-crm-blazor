using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class AuthGuardTests
{
    private readonly PlaywrightFixture _fixture;

    public AuthGuardTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UnauthenticatedUser_Dashboard_RedirectsToLogin()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForURLAsync("**/login**", new() { Timeout = 10000 });

        page.Url.Should().Contain("/login");
    }

    [Fact]
    public async Task UnauthenticatedUser_Contacts_RedirectsToLogin()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/contacts");
        await page.WaitForURLAsync("**/login**", new() { Timeout = 10000 });

        page.Url.Should().Contain("/login");
    }

    [Fact]
    public async Task UnauthenticatedUser_Companies_RedirectsToLogin()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/companies");
        await page.WaitForURLAsync("**/login**", new() { Timeout = 10000 });

        page.Url.Should().Contain("/login");
    }

    [Fact]
    public async Task UnauthenticatedUser_Deals_RedirectsToLogin()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/deals");
        await page.WaitForURLAsync("**/login**", new() { Timeout = 10000 });

        page.Url.Should().Contain("/login");
    }

    [Fact]
    public async Task LoginPage_IsAccessible_WithoutAuth()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        page.Url.Should().Contain("/login");
        var signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
        (await signInButton.IsVisibleAsync()).Should().BeTrue();
    }
}
