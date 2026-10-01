namespace OutletControl.Contracts.Receiving;

public class StockReceiptHistoryItemDto
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public string OutletName { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public decimal TotalAmount { get; set; }

    public int ProductsCount { get; set; }

    public int TotalQuantity { get; set; }

    public List<StockReceiptHistoryLineDto> Items { get; set; } = new();
}
