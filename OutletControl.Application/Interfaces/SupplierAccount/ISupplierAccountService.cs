using OutletControl.Contracts.SupplierAccount;

namespace OutletControl.Application.Interfaces.SupplierAccount;

public interface ISupplierAccountService
{
    Task<SupplierAccountDto> GetAccountAsync(
        int outletId);

    Task<SupplierAccountDto> AddPaymentAsync(
        CreateSupplierPaymentRequest request);

    Task<SupplierAccountLedgerDto> GetLedgerAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null);
}
