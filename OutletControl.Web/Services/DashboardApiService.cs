using System.Net.Http.Json;
using OutletControl.Contracts.Dashboard;

namespace OutletControl.Web.Services;

public class DashboardApiService
{
    private readonly HttpClient _httpClient;

    public DashboardApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OwnerDashboardDto?> GetOwnerDashboardAsync(int outletId)
    {
        return await _httpClient.GetFromJsonAsync<OwnerDashboardDto>(
            $"api/dashboard/owner/{outletId}");
    }
}