using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Blazor.Services.Api;
using SunroomCrm.Blazor.Services.Local;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration config)
    {
        var dataMode = config.GetValue<string>("DataMode") ?? "Local";

        if (dataMode.Equals("Api", StringComparison.OrdinalIgnoreCase))
        {
            RegisterApiServices(services, config);
        }
        else
        {
            RegisterLocalServices(services, config);
        }

        return services;
    }

    private static void RegisterLocalServices(IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAuthService, LocalAuthService>();
        services.AddScoped<IUserService, LocalUserService>();
        services.AddScoped<IContactService, LocalContactService>();
        services.AddScoped<ICompanyService, LocalCompanyService>();
        services.AddScoped<IDealService, LocalDealService>();
        services.AddScoped<IActivityService, LocalActivityService>();
        services.AddScoped<ITagService, LocalTagService>();
        services.AddScoped<IAiService, LocalAiService>();
        services.AddScoped<IDashboardService, LocalDashboardService>();
    }

    private static void RegisterApiServices(IServiceCollection services, IConfiguration config)
    {
        var apiBaseUrl = config.GetValue<string>("ApiBaseUrl")
            ?? "http://localhost:5000";

        services.AddTransient<JwtDelegatingHandler>();

        void ConfigureClient(HttpClient client) => client.BaseAddress = new Uri(apiBaseUrl);

        services.AddHttpClient<IAuthService, ApiAuthService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<IUserService, ApiUserService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<IContactService, ApiContactService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<ICompanyService, ApiCompanyService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<IDealService, ApiDealService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<IActivityService, ApiActivityService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<ITagService, ApiTagService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<IAiService, ApiAiService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
        services.AddHttpClient<IDashboardService, ApiDashboardService>(ConfigureClient)
            .AddHttpMessageHandler<JwtDelegatingHandler>();
    }
}
