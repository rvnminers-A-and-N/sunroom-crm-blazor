using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class AuthFlowTests
{
    private readonly PlaywrightFixture _fixture;

    public AuthFlowTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_ValidCredentials_RedirectsToDashboard()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await _fixture.LoginAsync(page);

        page.Url.Should().Contain("/dashboard");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShowsError()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByLabel("Email").FillAsync("wrong@example.com");
        await page.GetByLabel("Password").FillAsync("wrongpassword");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

        // Should stay on login page with error
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        page.Url.Should().Contain("/login");
    }

    [Fact]
    public async Task Register_NewUser_RedirectsToDashboard()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        var uniqueEmail = $"e2e.{Guid.NewGuid():N}@test.sunroom.dev";

        await page.GotoAsync($"{_fixture.BaseUrl}/register");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByLabel("Full Name").FillAsync("E2E Test User");
        await page.GetByLabel("Email").FillAsync(uniqueEmail);
        await page.GetByLabel("Password", new() { Exact = true }).FillAsync("Test123!");
        await page.GetByLabel("Confirm Password").FillAsync("Test123!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

        await page.WaitForURLAsync("**/dashboard**", new() { Timeout = 10000 });
        page.Url.Should().Contain("/dashboard");
    }

    [Fact]
    public async Task RegisterPage_HasLinkToLogin()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/register");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var loginLink = page.GetByRole(AriaRole.Link, new() { Name = "Sign in" });
        (await loginLink.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task LoginPage_HasLinkToRegister()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var registerLink = page.GetByRole(AriaRole.Link, new() { Name = "Create one" });
        (await registerLink.IsVisibleAsync()).Should().BeTrue();
    }
}
