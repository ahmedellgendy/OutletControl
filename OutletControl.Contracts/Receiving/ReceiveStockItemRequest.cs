namespace OutletControl.Contracts.Receiving;

public class ReceiveStockItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }
}