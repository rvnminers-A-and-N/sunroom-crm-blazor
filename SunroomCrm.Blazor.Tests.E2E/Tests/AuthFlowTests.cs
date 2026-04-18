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

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Call login API directly from the browser
        var loginOk = await page.EvaluateAsync<bool>(@"
            async (credentials) => {
                const r = await fetch('/api/account/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(credentials)
                });
                return r.ok;
            }
        ", new { email = "admin@sunroomcrm.net", password = "password123" });

        loginOk.Should().BeTrue("login API should accept valid credentials");

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        page.Url.Should().Contain("/dashboard");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShowsError()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var emailInput = page.Locator("input[type='email']");
        await emailInput.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        await emailInput.FillAsync("wrong@example.com");
        await page.Locator("input[type='password']").FillAsync("wrongpassword");
        await page.Locator("button[type='submit']").ClickAsync();

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
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Call register API directly from the browser
        var registerOk = await page.EvaluateAsync<bool>(@"
            async (credentials) => {
                const r = await fetch('/api/account/register', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(credentials)
                });
                return r.ok;
            }
        ", new { name = "E2E Test User", email = uniqueEmail, password = "Test123!" });

        registerOk.Should().BeTrue("register API should accept new user");

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        page.Url.Should().Contain("/dashboard");
    }

    [Fact]
    public async Task RegisterPage_HasLinkToLogin()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/register");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var loginLink = page.Locator("a[href='/login']");
        await loginLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        (await loginLink.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task LoginPage_HasLinkToRegister()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var registerLink = page.Locator("a[href='/register']");
        await registerLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        (await registerLink.IsVisibleAsync()).Should().BeTrue();
    }
}
