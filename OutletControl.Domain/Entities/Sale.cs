using OutletControl.Domain.Entities;

public class Sale
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public int ShiftId { get; set; }

    public string? ClientReferenceId { get; set; }

    public DateTime SoldAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public Outlet Outlet { get; set; } = null!;

    public Shift Shift { get; set; } = null!;

    public ICollection<SaleItem> Items { get; set; }
        = new List<SaleItem>();
}