namespace OutletControl.Contracts.SupplierAccount;

public class CreateSupplierPaymentRequest
{
    public int OutletId { get; set; }

    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}