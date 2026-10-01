namespace OutletControl.Contracts.Dashboard;

public class OwnerDashboardDto
{
    public int OutletId { get; set; }

    // Sales
    public decimal TodaySales { get; set; }

    public int TodaySalesCount { get; set; }

    public decimal TodaySalesCost { get; set; }

    public decimal TodayGrossProfit { get; set; }

    public decimal TodayGrossMarginPercent { get; set; }

    // Receiving
    public decimal TodayReceivingAmount { get; set; }

    // Stock
    public int CurrentStockUnits { get; set; }

    public decimal CurrentStockCostValue { get; set; }

    public decimal CurrentStockRetailValue { get; set; }

    public decimal CurrentStockExpectedProfit { get; set; }

    public decimal CurrentStockExpectedMarginPercent { get; set; }

    public int LowStockProductsCount { get; set; }

    // Financial
    public decimal CurrentDebt { get; set; }

    public bool HasOpenShift { get; set; }

    public decimal CurrentExpectedCash { get; set; }

    // Expenses / treasury
    // This remains zero until the treasury/expense module is implemented.
    public decimal TodayExpenses { get; set; }

    public decimal TodayNetProfit { get; set; }

    public decimal TodayNetMarginPercent { get; set; }
}
