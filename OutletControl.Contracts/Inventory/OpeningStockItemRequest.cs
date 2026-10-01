namespace OutletControl.Contracts.Inventory;

public class OpeningStockItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}