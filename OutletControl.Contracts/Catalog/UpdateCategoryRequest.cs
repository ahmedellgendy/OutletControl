namespace OutletControl.Contracts.Catalog;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}