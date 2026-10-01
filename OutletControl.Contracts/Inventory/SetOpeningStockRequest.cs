namespace OutletControl.Contracts.Inventory;

public class SetOpeningStockRequest
{
    public int OutletId { get; set; }

    public List<OpeningStockItemRequest> Items { get; set; } = new();
}