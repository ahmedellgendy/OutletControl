namespace OutletControl.Contracts.Treasury;

public class TreasuryMovementDto
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public int? ShiftId { get; set; }

    public TreasuryMovementTypeDto Type { get; set; }

    public decimal Amount { get; set; }

    public DateTime OccurredAt { get; set; }

    public string? Category { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }
}
