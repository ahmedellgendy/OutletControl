namespace OutletControl.Contracts.Inventory;

public class StockBalanceDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal AverageUnitCost { get; set; }

    public decimal CostValue =>
        Quantity * AverageUnitCost;

    public decimal RetailValue =>
        Quantity * SellingPrice;
}