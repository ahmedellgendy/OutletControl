using OutletControl.Contracts.Dashboard;

namespace OutletControl.Application.Interfaces.Dashboard;

public interface IDashboardService
{
    Task<OwnerDashboardDto> GetOwnerDashboardAsync(int outletId);
}