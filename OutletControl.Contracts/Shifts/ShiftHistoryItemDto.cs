namespace OutletControl.Contracts.Shifts;

public class ShiftHistoryItemDto
{
    public int Id { get; set; }

    public int OutletId { get; set; }

    public string OutletName { get; set; } = string.Empty;

    public DateTime OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public decimal OpeningCash { get; set; }

    public decimal SalesAmount { get; set; }

    public decimal ExpectedCash { get; set; }

    public decimal? ActualCash { get; set; }

    public decimal? CashDifference { get; set; }

    public string? Notes { get; set; }

    public bool IsClosed { get; set; }
}
