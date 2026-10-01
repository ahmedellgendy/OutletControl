namespace OutletControl.Contracts.Receiving;

public class StockReceiptDto
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public DateTime ReceivedAt { get; set; }

    public string? ReferenceNumber { get; set; }

    public decimal TotalAmount { get; set; }
}