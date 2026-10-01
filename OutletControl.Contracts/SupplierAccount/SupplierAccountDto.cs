namespace OutletControl.Contracts.SupplierAccount;

public class SupplierAccountDto
{
    public int OutletId { get; set; }

    public decimal TotalReceived { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal CurrentDebt { get; set; }
}