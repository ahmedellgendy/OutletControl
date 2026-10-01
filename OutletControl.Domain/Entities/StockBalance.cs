namespace OutletControl.Domain.Entities;

public class StockBalance
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal AverageUnitCost { get; set; }

    // SQL Server RowVersion
    // Used for optimistic concurrency protection.
    public byte[] RowVersion { get; set; } =
        Array.Empty<byte>();

    public Outlet Outlet { get; set; } = null!;

    public Product Product { get; set; } = null!;
}