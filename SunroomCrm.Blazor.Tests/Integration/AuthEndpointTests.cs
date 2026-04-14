using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Auth;

namespace SunroomCrm.Blazor.Tests.Integration;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTest_Auth"));
            });
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        SeedData.SeedAsync(db).GetAwaiter().GetResult();
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/register", new RegisterRequest
        {
            Name = "Integration Test " + Guid.NewGuid(),
            Email = $"integration-{Guid.NewGuid()}@test.com",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithCookie()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest
        {
            Email = "admin@sunroomcrm.com",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest
        {
            Email = "admin@sunroomcrm.com",
            Password = "wrongpassword"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/account/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
