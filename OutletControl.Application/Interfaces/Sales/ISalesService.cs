using OutletControl.Contracts.Sales;

namespace OutletControl.Application.Interfaces.Sales;

public interface ISalesService
{
    Task<SaleDto> CreateSaleAsync(
        CreateSaleRequest request);

    Task<IReadOnlyList<SaleHistoryItemDto>> GetHistoryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null);
}
