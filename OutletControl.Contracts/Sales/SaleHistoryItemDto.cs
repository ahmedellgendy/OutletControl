namespace OutletControl.Contracts.Sales;

public class SaleHistoryItemDto
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public string OutletName { get; set; } = string.Empty;

    public int ShiftId { get; set; }

    public DateTime SoldAt { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal TotalCost { get; set; }

    public decimal GrossProfit { get; set; }

    public int ProductsCount { get; set; }

    public int TotalQuantity { get; set; }

    public List<SaleHistoryLineDto> Items { get; set; } = new();
}
