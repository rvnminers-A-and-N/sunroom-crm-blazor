using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SunroomCrm.Blazor.Services.Api;

namespace SunroomCrm.Blazor.Tests.Unit.Services.Api;

public class JwtDelegatingHandlerTests
{
    private class TestHandler : DelegatingHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task SendAsync_WithJwtClaim_AddsAuthorizationHeader()
    {
        var claims = new List<Claim> { new("jwt", "test-token-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var innerHandler = new TestHandler();
        var handler = new JwtDelegatingHandler(mockAccessor.Object) { InnerHandler = innerHandler };

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        await client.GetAsync("/test");

        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        innerHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        innerHandler.LastRequest.Headers.Authorization.Parameter.Should().Be("test-token-123");
    }

    [Fact]
    public async Task SendAsync_WithoutJwtClaim_NoAuthorizationHeader()
    {
        var httpContext = new DefaultHttpContext();

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var innerHandler = new TestHandler();
        var handler = new JwtDelegatingHandler(mockAccessor.Object) { InnerHandler = innerHandler };

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        await client.GetAsync("/test");

        innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_NullHttpContext_NoAuthorizationHeader()
    {
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var innerHandler = new TestHandler();
        var handler = new JwtDelegatingHandler(mockAccessor.Object) { InnerHandler = innerHandler };

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };
        await client.GetAsync("/test");

        innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }
}
