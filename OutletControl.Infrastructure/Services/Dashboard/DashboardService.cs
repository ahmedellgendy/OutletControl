using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Dashboard;
using OutletControl.Contracts.Dashboard;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly OutletControlDbContext _context;

    public DashboardService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<OwnerDashboardDto> GetOwnerDashboardAsync(
        int outletId)
    {
        var outletExists =
            await _context.Outlets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == outletId &&
                    x.IsActive);

        if (!outletExists)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود.");
        }

        var todayLocal =
            DateTime.Today;

        var today =
            todayLocal.ToUniversalTime();

        var tomorrow =
            todayLocal
                .AddDays(1)
                .ToUniversalTime();

        var todaySalesQuery =
            _context.Sales
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId &&
                    x.SoldAt >= today &&
                    x.SoldAt < tomorrow);

        var todaySales =
            await todaySalesQuery
                .SumAsync(x =>
                    (decimal?)x.TotalAmount)
            ?? 0m;

        var todaySalesCount =
            await todaySalesQuery
                .CountAsync();

        var todaySalesCost =
            await _context.SaleItems
                .AsNoTracking()
                .Where(x =>
                    x.Sale.OutletId == outletId &&
                    x.Sale.SoldAt >= today &&
                    x.Sale.SoldAt < tomorrow)
                .SumAsync(x =>
                    (decimal?)
                    (x.Quantity *
                     x.UnitCost))
            ?? 0m;

        var todayGrossProfit =
            todaySales -
            todaySalesCost;

        var todayGrossMarginPercent =
            todaySales > 0
                ? Math.Round(
                    (todayGrossProfit /
                     todaySales) * 100m,
                    2)
                : 0m;

        var todayReceivingAmount =
            await _context.StockReceipts
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId &&
                    x.ReceivedAt >= today &&
                    x.ReceivedAt < tomorrow)
                .SumAsync(x =>
                    (decimal?)x.TotalAmount)
            ?? 0m;

        var stockSummary =
            await _context.StockBalances
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId)
                .GroupBy(_ => 1)
                .Select(g =>
                    new
                    {
                        Units =
                            g.Sum(x =>
                                x.Quantity),

                        CostValue =
                            g.Sum(x =>
                                x.Quantity *
                                x.AverageUnitCost),

                        RetailValue =
                            g.Sum(x =>
                                x.Quantity *
                                x.Product.SellingPrice)
                    })
                .FirstOrDefaultAsync();

        var currentStockUnits =
            stockSummary?.Units ?? 0;

        var currentStockCostValue =
            stockSummary?.CostValue ?? 0m;

        var currentStockRetailValue =
            stockSummary?.RetailValue ?? 0m;

        var currentStockExpectedProfit =
            currentStockRetailValue -
            currentStockCostValue;

        var currentStockExpectedMarginPercent =
            currentStockRetailValue > 0
                ? Math.Round(
                    (currentStockExpectedProfit /
                     currentStockRetailValue) * 100m,
                    2)
                : 0m;

        var lowStockProductsCount =
            await _context.StockBalances
                .AsNoTracking()
                .CountAsync(x =>
                    x.OutletId == outletId &&
                    x.Quantity <= 5);

        var totalReceived =
            await _context.StockReceipts
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId)
                .SumAsync(x =>
                    (decimal?)x.TotalAmount)
            ?? 0m;

        var totalPaid =
            await _context.SupplierPayments
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0m;

        var currentDebt =
            totalReceived -
            totalPaid;

        var openShift =
            await _context.Shifts
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.OutletId == outletId &&
                    !x.IsClosed);

        var todayExpenses =
            await _context.TreasuryMovements
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId &&
                    x.Type ==
                        TreasuryMovementType.Expense &&
                    x.OccurredAt >= today &&
                    x.OccurredAt < tomorrow)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0m;

        var todayNetProfit =
            todayGrossProfit -
            todayExpenses;

        var todayNetMarginPercent =
            todaySales > 0
                ? Math.Round(
                    (todayNetProfit /
                     todaySales) * 100m,
                    2)
                : 0m;

        decimal currentExpectedCash =
            0m;

        if (openShift is not null)
        {
            var currentShiftSales =
                await _context.Sales
                    .AsNoTracking()
                    .Where(x =>
                        x.ShiftId ==
                        openShift.Id)
                    .SumAsync(x =>
                        (decimal?)x.TotalAmount)
                ?? 0m;

            currentExpectedCash =
                openShift.OpeningCash +
                currentShiftSales;
        }

        return new OwnerDashboardDto
        {
            OutletId =
                outletId,

            TodaySales =
                todaySales,

            TodaySalesCount =
                todaySalesCount,

            TodaySalesCost =
                todaySalesCost,

            TodayGrossProfit =
                todayGrossProfit,

            TodayGrossMarginPercent =
                todayGrossMarginPercent,

            TodayReceivingAmount =
                todayReceivingAmount,

            CurrentStockUnits =
                currentStockUnits,

            CurrentStockCostValue =
                currentStockCostValue,

            CurrentStockRetailValue =
                currentStockRetailValue,

            CurrentStockExpectedProfit =
                currentStockExpectedProfit,

            CurrentStockExpectedMarginPercent =
                currentStockExpectedMarginPercent,

            LowStockProductsCount =
                lowStockProductsCount,

            CurrentDebt =
                currentDebt,

            HasOpenShift =
                openShift is not null,

            CurrentExpectedCash =
                currentExpectedCash,

            TodayExpenses =
                todayExpenses,

            TodayNetProfit =
                todayNetProfit,

            TodayNetMarginPercent =
                todayNetMarginPercent
        };
    }
}
