using OutletControl.Contracts.Shifts;

namespace OutletControl.Application.Interfaces.Shifts;

public interface IShiftService
{
    Task<ShiftDto> OpenShiftAsync(
        OpenShiftRequest request);

    Task<ShiftDto> CloseShiftAsync(
        CloseShiftRequest request);

    Task<ShiftDto?> GetOpenShiftAsync(
        int outletId);

    Task<IReadOnlyList<ShiftHistoryItemDto>> GetHistoryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null,
        bool? isClosed = null);
}
