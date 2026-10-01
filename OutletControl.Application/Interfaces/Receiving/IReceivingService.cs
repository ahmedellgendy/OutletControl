using OutletControl.Contracts.Receiving;

namespace OutletControl.Application.Interfaces.Receiving;

public interface IReceivingService
{
    Task<StockReceiptDto> ReceiveAsync(
        ReceiveStockRequest request);

    Task<IReadOnlyList<StockReceiptHistoryItemDto>> GetHistoryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null);
}
