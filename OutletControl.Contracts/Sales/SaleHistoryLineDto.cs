namespace OutletControl.Contracts.Sales;

public class SaleHistoryLineDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal TotalCost { get; set; }

    public decimal GrossProfit { get; set; }
}
