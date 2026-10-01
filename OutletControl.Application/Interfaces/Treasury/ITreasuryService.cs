using OutletControl.Contracts.Treasury;

namespace OutletControl.Application.Interfaces.Treasury;

public interface ITreasuryService
{
    Task<TreasuryMovementDto> CreateMovementAsync(
        CreateTreasuryMovementRequest request,
        int? createdByUserId);

    Task<TreasuryPageDto> GetTreasuryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null);

    Task<decimal> GetBalanceAsync(
        int outletId);
}
