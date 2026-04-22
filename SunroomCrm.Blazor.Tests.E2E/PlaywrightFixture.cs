using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using MudBlazor.Services;
using SunroomCrm.Blazor.Auth;
using SunroomCrm.Blazor.Components;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Blazor.Services;
using Xunit;

namespace SunroomCrm.Blazor.Tests.E2E;

public class PlaywrightFixture : IAsyncLifetime
{
    private WebApplication? _app;
    private IPlaywright? _playwright;
    public string BaseUrl { get; private set; } = "";
    public IPlaywright Playwright => _playwright!;
    public bool IsIntegrationMode { get; private set; }

    public async Task InitializeAsync()
    {
        var apiUrl = Environment.GetEnvironmentVariable("BLAZOR_E2E_API_URL");
        IsIntegrationMode = !string.IsNullOrEmpty(apiUrl);

        // Resolve paths for static web assets and wwwroot
        var testBinDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(testBinDir, "..", "..", "..", ".."));
        var blazorProjectDir = Path.Combine(repoRoot, "SunroomCrm.Blazor");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = Directory.Exists(blazorProjectDir) ? blazorProjectDir : Directory.GetCurrentDirectory(),
            WebRootPath = Directory.Exists(Path.Combine(blazorProjectDir, "wwwroot"))
                ? Path.Combine(blazorProjectDir, "wwwroot") : null,
            ApplicationName = typeof(SunroomCrm.Blazor.Components.App).Assembly.GetName().Name!,
            EnvironmentName = "Development"
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        if (IsIntegrationMode)
        {
            // Integration mode: connect to real .NET API
            builder.Configuration["DataMode"] = "Api";
            builder.Configuration["ApiBaseUrl"] = apiUrl;
        }
        else
        {
            // Local mode: InMemory database
            builder.Configuration["DataMode"] = "Local";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = "Server=dummy";
        }

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(options => options.DetailedErrors = true)
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddMudServices();
        builder.Services.AddDataServices(builder.Configuration);

        if (!IsIntegrationMode)
        {
            // Replace SQL Server DbContext with InMemory
            var descriptor = builder.Services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) builder.Services.Remove(descriptor);

            var dbName = $"E2E_{Guid.NewGuid()}";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        }

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCascadingAuthenticationState();

        _app = builder.Build();

        _app.UseStaticFiles();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.UseAntiforgery();
        _app.MapAuthEndpoints();

        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(SunroomCrm.Blazor.Client._Imports).Assembly);

        await _app.StartAsync();
        BaseUrl = _app.Urls.First();

        if (!IsIntegrationMode)
        {
            // Seed data only in local mode
            using var scope = _app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedData.SeedAsync(db);
        }

        // Initialize Playwright
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
    }

    public async Task<IBrowserContext> CreateBrowserContextAsync(string browserType = "chromium")
    {
        var browser = browserType switch
        {
            "firefox" => await Playwright.Firefox.LaunchAsync(),
            "webkit" => await Playwright.Webkit.LaunchAsync(),
            _ => await Playwright.Chromium.LaunchAsync()
        };

        return await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        });
    }

    public async Task<IPage> LoginAsync(IPage page, string email = "admin@sunroomcrm.net", string password = "password123")
    {
        // Navigate to establish same-origin context
        await page.GotoAsync($"{BaseUrl}/login");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Login via direct API call — bypasses Blazor circuit timing issues
        // in CI where InteractiveServer WebSocket may not connect in time
        var loginOk = await page.EvaluateAsync<bool>(@"
            async (credentials) => {
                const r = await fetch('/api/account/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(credentials)
                });
                return r.ok;
            }
        ", new { email, password });

        if (!loginOk)
            throw new InvalidOperationException($"E2E login failed for {email}");

        // Navigate to dashboard with the auth cookie now set
        await page.GotoAsync($"{BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        return page;
    }

    public async Task DisposeAsync()
    {
        _playwright?.Dispose();
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
