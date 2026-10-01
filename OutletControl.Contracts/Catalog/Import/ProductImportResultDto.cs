namespace OutletControl.Contracts.Catalog.Import;

public class ProductImportResultDto
{
    public int TotalRows { get; set; }

    public int ImportedRows { get; set; }

    public int FailedRows { get; set; }

    public List<ProductImportErrorDto> Errors { get; set; } = new();
}

public class ProductImportErrorDto
{
    public int RowNumber { get; set; }

    public string? ProductName { get; set; }

    public string Message { get; set; } = string.Empty;
}