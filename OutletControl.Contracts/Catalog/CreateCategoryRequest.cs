namespace OutletControl.Contracts.Catalog;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}