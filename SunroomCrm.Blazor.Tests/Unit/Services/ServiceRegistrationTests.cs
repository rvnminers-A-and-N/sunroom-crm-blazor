using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SunroomCrm.Blazor.Services;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Tests.Unit.Services;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddDataServices_LocalMode_RegistersLocalServices()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataMode"] = "Local",
                ["ConnectionStrings:DefaultConnection"] = "Server=dummy"
            })
            .Build();

        services.AddDataServices(config);
        var provider = services.BuildServiceProvider();

        services.Should().Contain(s => s.ServiceType == typeof(IAuthService)
            && s.ImplementationType == typeof(LocalAuthService));
        services.Should().Contain(s => s.ServiceType == typeof(IContactService)
            && s.ImplementationType == typeof(LocalContactService));
        services.Should().Contain(s => s.ServiceType == typeof(ICompanyService)
            && s.ImplementationType == typeof(LocalCompanyService));
        services.Should().Contain(s => s.ServiceType == typeof(IDealService)
            && s.ImplementationType == typeof(LocalDealService));
        services.Should().Contain(s => s.ServiceType == typeof(IActivityService)
            && s.ImplementationType == typeof(LocalActivityService));
        services.Should().Contain(s => s.ServiceType == typeof(ITagService)
            && s.ImplementationType == typeof(LocalTagService));
        services.Should().Contain(s => s.ServiceType == typeof(IAiService)
            && s.ImplementationType == typeof(LocalAiService));
        services.Should().Contain(s => s.ServiceType == typeof(IDashboardService)
            && s.ImplementationType == typeof(LocalDashboardService));
        services.Should().Contain(s => s.ServiceType == typeof(IUserService)
            && s.ImplementationType == typeof(LocalUserService));
    }

    [Fact]
    public void AddDataServices_ApiMode_RegistersApiServices()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataMode"] = "Api",
                ["ApiBaseUrl"] = "http://localhost:5236"
            })
            .Build();

        services.AddDataServices(config);

        // HttpClient-based registrations use IHttpClientFactory pattern,
        // so we check for the handler registration
        services.Should().Contain(s => s.ServiceType == typeof(JwtDelegatingHandler));
    }

    [Fact]
    public void AddDataServices_DefaultMode_FallsBackToLocal()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=dummy"
            })
            .Build();

        services.AddDataServices(config);

        services.Should().Contain(s => s.ServiceType == typeof(IAuthService)
            && s.ImplementationType == typeof(LocalAuthService));
    }

    [Fact]
    public void AddDataServices_ApiMode_NoBaseUrl_UsesDefault()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataMode"] = "Api"
            })
            .Build();

        services.AddDataServices(config);

        services.Should().Contain(s => s.ServiceType == typeof(JwtDelegatingHandler));
    }
}
