namespace OutletControl.Domain.Entities;

public class StockReceipt
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public string? ClientReferenceId { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public Outlet Outlet { get; set; } = null!;

    public ICollection<StockReceiptItem> Items { get; set; }
        = new List<StockReceiptItem>();
}