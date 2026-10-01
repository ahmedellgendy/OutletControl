namespace OutletControl.Contracts.Treasury;

public class TreasurySummaryDto
{
    public int OutletId { get; set; }

    public decimal CurrentBalance { get; set; }

    public decimal TotalCashIn { get; set; }

    public decimal TotalShiftSettlements { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal TotalCashOut { get; set; }

    public decimal TotalSupplierPayments { get; set; }

    public decimal TotalShiftOpenings { get; set; }
}
