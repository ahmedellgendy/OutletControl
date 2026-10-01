namespace OutletControl.Contracts.Receiving;

public class StockReceiptHistoryLineDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }
}
