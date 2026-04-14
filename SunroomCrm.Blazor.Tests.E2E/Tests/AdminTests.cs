using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class AdminTests
{
    private readonly PlaywrightFixture _fixture;

    public AdminTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AdminPage_AdminUser_CanAccess()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GotoAsync($"{_fixture.BaseUrl}/admin/users");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Admin user should be able to access user management
        page.Url.Should().Contain("/admin/users");
    }

    [Fact]
    public async Task AdminLink_VisibleForAdminUser()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        var adminLink = page.GetByRole(AriaRole.Link, new() { Name = "Admin" });
        (await adminLink.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task SettingsPage_IsAccessible()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
        await page.WaitForURLAsync("**/settings/**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        page.Url.Should().Contain("/settings/profile");
    }
}
