namespace OutletControl.Contracts.Catalog.Import;

public class ProductImportPreviewDto
{
    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int InvalidRows { get; set; }

    public List<ProductImportRowDto> Rows { get; set; } = new();
}