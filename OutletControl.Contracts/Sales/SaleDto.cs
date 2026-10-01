namespace OutletControl.Contracts.Sales;

public class SaleDto
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public DateTime SoldAt { get; set; }

    public decimal TotalAmount { get; set; }

    public List<SaleItemDto> Items { get; set; } = new();
}