namespace OutletControl.Domain.Entities;

public class Shift
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    public decimal OpeningCash { get; set; }

    public decimal SalesAmount { get; set; }

    public decimal ExpectedCash { get; set; }

    public decimal? ActualCash { get; set; }

    public decimal? CashDifference { get; set; }

    public string? Notes { get; set; }

    public bool IsClosed { get; set; }

    public Outlet Outlet { get; set; } = null!;

    public ICollection<Sale> Sales { get; set; }
        = new List<Sale>();

    public ICollection<TreasuryMovement> TreasuryMovements { get; set; }
        = new List<TreasuryMovement>();
}
