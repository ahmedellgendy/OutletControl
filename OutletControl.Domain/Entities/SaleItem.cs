namespace OutletControl.Domain.Entities;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    // Snapshot of the product cost at the moment of sale.
    // This makes historical gross profit accurate even if cost changes later.
    public decimal UnitCost { get; set; }

    public decimal TotalAmount { get; set; }

    public Sale Sale { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
