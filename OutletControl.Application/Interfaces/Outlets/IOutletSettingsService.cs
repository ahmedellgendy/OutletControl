using OutletControl.Contracts.Outlets;

namespace OutletControl.Application.Interfaces.Outlets;

public interface IOutletSettingsService
{
    Task<OutletSettingsDto> GetAsync(
        int outletId);

    Task<OutletSettingsDto> UpdateAsync(
        int outletId,
        UpdateOutletSettingsRequest request);
}
