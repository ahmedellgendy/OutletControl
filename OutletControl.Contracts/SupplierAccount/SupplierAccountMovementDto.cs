namespace OutletControl.Contracts.SupplierAccount;

public class SupplierAccountMovementDto
{
    public int Id { get; set; }

    public SupplierAccountMovementType Type { get; set; }

    public DateTime OccurredAt { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }
}
