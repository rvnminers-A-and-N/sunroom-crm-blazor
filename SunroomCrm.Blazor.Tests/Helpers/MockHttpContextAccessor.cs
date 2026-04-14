using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;

namespace SunroomCrm.Blazor.Tests.Helpers;

public static class MockHttpContextAccessor
{
    public static IHttpContextAccessor Create(int userId = 1)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        return mock.Object;
    }

    public static IHttpContextAccessor CreateWithNullContext()
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        return mock.Object;
    }

    public static IHttpContextAccessor CreateWithNullUser()
    {
        var context = new Mock<HttpContext>();
        context.Setup(x => x.User).Returns((ClaimsPrincipal)null!);
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(context.Object);
        return mock.Object;
    }
}
