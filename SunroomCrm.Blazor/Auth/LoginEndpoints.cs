using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SunroomCrm.Shared.DTOs.Auth;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Auth;

public static class LoginEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/account/login", async (LoginRequest request, IAuthService authService, HttpContext httpContext) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                await SignInUser(httpContext, response);
                return Results.Ok(response.User);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        }).AllowAnonymous();

        app.MapPost("/api/account/register", async (RegisterRequest request, IAuthService authService, HttpContext httpContext) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                await SignInUser(httpContext, response);
                return Results.Ok(response.User);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).AllowAnonymous();

        app.MapPost("/api/account/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        });
    }

    private static async Task SignInUser(HttpContext httpContext, AuthResponse response)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
            new(ClaimTypes.Name, response.User.Name),
            new(ClaimTypes.Email, response.User.Email),
            new(ClaimTypes.Role, response.User.Role)
        };

        if (!string.IsNullOrEmpty(response.Token))
            claims.Add(new Claim("jwt", response.Token));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
    }
}
