using OutletControl.Domain.Enums;

namespace OutletControl.Domain.Entities;

public class TreasuryMovement
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public int? ShiftId { get; set; }

    public TreasuryMovementType Type { get; set; }

    public decimal Amount { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public string? Category { get; set; }

    public string? Notes { get; set; }

    // Audit field only. We intentionally keep this as a scalar so
    // the Domain project does not depend on the Identity implementation.
    public int? CreatedByUserId { get; set; }

    // Reserved for linking a movement to another module later
    // (for example a SupplierPayment) without changing the treasury model.
    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public Outlet Outlet { get; set; } = null!;

    public Shift? Shift { get; set; }
}
