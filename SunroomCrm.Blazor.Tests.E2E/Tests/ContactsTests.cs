using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E.Tests;

[Collection("Playwright")]
public class ContactsTests
{
    private readonly PlaywrightFixture _fixture;

    public ContactsTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ContactsList_ShowsSeededContacts()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableRows = page.Locator("table tbody tr");
        (await tableRows.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ContactsList_HasSearchField()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var searchInput = page.GetByPlaceholder("Search");
        (await searchInput.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task ContactsList_HasCreateButton()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var createButton = page.GetByRole(AriaRole.Button, new() { Name = "New Contact" });
        (await createButton.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task ContactsList_CreateButton_OpensDialog()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByRole(AriaRole.Button, new() { Name = "New Contact" }).ClickAsync();

        var dialog = page.GetByRole(AriaRole.Dialog);
        (await dialog.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task ContactsList_ClickRow_NavigatesToDetail()
    {
        await using var context = await _fixture.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();
        await _fixture.LoginAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Contacts" }).ClickAsync();
        await page.WaitForURLAsync("**/contacts**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstRow = page.Locator("table tbody tr").First;
        await firstRow.ClickAsync();

        await page.WaitForURLAsync("**/contacts/**");
        page.Url.Should().MatchRegex(@"/contacts/\d+");
    }
}
