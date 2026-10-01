using OutletControl.Domain.Enums;


namespace OutletControl.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public StockMovementType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    public Outlet Outlet { get; set; } = null!;

    public Product Product { get; set; } = null!;
}