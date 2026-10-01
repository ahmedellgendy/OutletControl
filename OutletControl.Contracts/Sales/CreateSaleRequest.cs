namespace OutletControl.Contracts.Sales;

public class CreateSaleRequest
{
    public int OutletId { get; set; }

    public string ClientReferenceId { get; set; } =
        string.Empty;

    public List<CreateSaleItemRequest> Items { get; set; } =
        new();
}