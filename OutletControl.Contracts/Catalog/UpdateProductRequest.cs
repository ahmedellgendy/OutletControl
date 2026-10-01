namespace OutletControl.Contracts.Catalog;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public decimal SellingPrice { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public int CategoryId { get; set; }

    public bool IsActive { get; set; }
}