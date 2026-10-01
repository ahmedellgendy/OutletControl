namespace OutletControl.Domain.Entities;

public class SupplierPayment
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    public Outlet Outlet { get; set; } = null!;
}