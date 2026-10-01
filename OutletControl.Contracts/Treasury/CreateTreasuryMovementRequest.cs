namespace OutletControl.Contracts.Treasury;

public class CreateTreasuryMovementRequest
{
    public int OutletId { get; set; }

    public TreasuryMovementTypeDto Type { get; set; }

    public decimal Amount { get; set; }

    public string? Category { get; set; }

    public string? Notes { get; set; }
}
