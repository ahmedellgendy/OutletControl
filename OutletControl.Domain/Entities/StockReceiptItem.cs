namespace OutletControl.Domain.Entities;

public class StockReceiptItem
{
    public int Id { get; set; }

    public int StockReceiptId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost => Quantity * UnitCost;

    public StockReceipt StockReceipt { get; set; } = null!;

    public Product Product { get; set; } = null!;
}