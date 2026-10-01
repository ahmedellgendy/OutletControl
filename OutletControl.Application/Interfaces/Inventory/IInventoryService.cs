using OutletControl.Contracts.Inventory;

namespace OutletControl.Application.Interfaces.Inventory;

public interface IInventoryService
{
    Task SetOpeningStockAsync(
        SetOpeningStockRequest request);

    Task<IReadOnlyList<StockBalanceDto>> GetStockAsync(
        int outletId,
        bool includeFinancialData);

    Task<IReadOnlyList<OpeningStockProductDto>>
        GetOpeningStockProductsAsync(
            int outletId);
}