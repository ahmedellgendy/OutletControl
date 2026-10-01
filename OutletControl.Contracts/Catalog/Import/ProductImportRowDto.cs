namespace OutletControl.Contracts.Catalog.Import;

public class ProductImportRowDto
{
    public int RowNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal SellingPrice { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ImageUrl { get; set; }

    public bool IsValid { get; set; }

    public List<string> Errors { get; set; } = new();
}