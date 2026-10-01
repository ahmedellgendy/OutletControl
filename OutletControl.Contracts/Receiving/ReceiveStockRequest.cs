namespace OutletControl.Contracts.Receiving;

public class ReceiveStockRequest
{
    public int OutletId { get; set; }

    public string ClientReferenceId { get; set; } =
        string.Empty;

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public List<ReceiveStockItemRequest> Items { get; set; } =
        new();
}