using System.Net.Http.Json;
using SunroomCrm.Shared.DTOs.Dashboard;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Api;

public class ApiDashboardService : IDashboardService
{
    private readonly HttpClient _http;

    public ApiDashboardService(HttpClient http)
    {
        _http = http;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        return await _http.GetFromJsonAsync<DashboardDto>("api/dashboard")
            ?? new DashboardDto();
    }
}
