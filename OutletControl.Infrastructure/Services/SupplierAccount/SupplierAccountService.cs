using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.SupplierAccount;
using OutletControl.Contracts.SupplierAccount;
using OutletControl.Domain.Entities;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.SupplierAccount;

public class SupplierAccountService : ISupplierAccountService
{
    private readonly OutletControlDbContext _context;

    public SupplierAccountService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierAccountDto> GetAccountAsync(
        int outletId)
    {
        await EnsureOutletExistsAsync(
            outletId);

        var totalReceived =
            await _context.StockReceipts
                .Where(x =>
                    x.OutletId == outletId)
                .SumAsync(x =>
                    (decimal?)x.TotalAmount)
            ?? 0m;

        var totalPaid =
            await _context.SupplierPayments
                .Where(x =>
                    x.OutletId == outletId)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0m;

        return new SupplierAccountDto
        {
            OutletId =
                outletId,

            TotalReceived =
                totalReceived,

            TotalPaid =
                totalPaid,

            CurrentDebt =
                totalReceived -
                totalPaid
        };
    }

    public async Task<SupplierAccountDto> AddPaymentAsync(
        CreateSupplierPaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "قيمة الدفعة يجب أن تكون أكبر من صفر.");
        }

        await EnsureOutletExistsAsync(
            request.OutletId);

        var account =
            await GetAccountAsync(
                request.OutletId);

        if (request.Amount >
            account.CurrentDebt)
        {
            throw new InvalidOperationException(
                $"قيمة الدفعة أكبر من المديونية الحالية ({account.CurrentDebt:0.##} ج).");
        }

        var treasuryBalance =
            await GetTreasuryBalanceAsync(
                request.OutletId);

        if (request.Amount >
            treasuryBalance)
        {
            throw new InvalidOperationException(
                $"رصيد الخزنة غير كافٍ. الرصيد الحالي {treasuryBalance:0.##} ج.");
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var payment =
                new SupplierPayment
                {
                    OutletId =
                        request.OutletId,

                    Amount =
                        request.Amount,

                    Notes =
                        string.IsNullOrWhiteSpace(
                            request.Notes)
                            ? null
                            : request.Notes.Trim(),

                    PaidAt =
                        DateTime.UtcNow
                };

            _context.SupplierPayments.Add(
                payment);

            await _context.SaveChangesAsync();

            _context.TreasuryMovements.Add(
                new TreasuryMovement
                {
                    OutletId =
                        request.OutletId,

                    ShiftId =
                        null,

                    Type =
                        TreasuryMovementType.SupplierPayment,

                    Amount =
                        request.Amount,

                    OccurredAt =
                        payment.PaidAt,

                    Category =
                        "سداد الشركة",

                    Notes =
                        payment.Notes,

                    ReferenceType =
                        nameof(SupplierPayment),

                    ReferenceId =
                        payment.Id
                });

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return await GetAccountAsync(
            request.OutletId);
    }

    public async Task<SupplierAccountLedgerDto> GetLedgerAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null)
    {
        await EnsureOutletExistsAsync(
            outletId);

        var receiptsQuery =
            _context.StockReceipts
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId);

        var paymentsQuery =
            _context.SupplierPayments
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId);

        if (toUtcExclusive.HasValue)
        {
            receiptsQuery =
                receiptsQuery.Where(x =>
                    x.ReceivedAt <
                    toUtcExclusive.Value);

            paymentsQuery =
                paymentsQuery.Where(x =>
                    x.PaidAt <
                    toUtcExclusive.Value);
        }

        var receipts =
            await receiptsQuery
                .Select(x =>
                    new LedgerSourceItem
                    {
                        Id =
                            x.Id,

                        Type =
                            SupplierAccountMovementType
                                .Receiving,

                        OccurredAt =
                            x.ReceivedAt,

                        Amount =
                            x.TotalAmount,

                        ReferenceNumber =
                            x.ReferenceNumber,

                        Notes =
                            x.Notes
                    })
                .ToListAsync();

        var payments =
            await paymentsQuery
                .Select(x =>
                    new LedgerSourceItem
                    {
                        Id =
                            x.Id,

                        Type =
                            SupplierAccountMovementType
                                .Payment,

                        OccurredAt =
                            x.PaidAt,

                        Amount =
                            x.Amount,

                        ReferenceNumber =
                            null,

                        Notes =
                            x.Notes
                    })
                .ToListAsync();

        var allMovements =
            receipts
                .Concat(payments)
                .OrderBy(x =>
                    x.OccurredAt)
                .ThenBy(x =>
                    x.Type)
                .ThenBy(x =>
                    x.Id)
                .ToList();

        decimal runningBalance =
            0m;

        var calculatedMovements =
            new List<SupplierAccountMovementDto>(
                allMovements.Count);

        foreach (var movement in allMovements)
        {
            if (movement.Type ==
                SupplierAccountMovementType.Receiving)
            {
                runningBalance +=
                    movement.Amount;
            }
            else
            {
                runningBalance -=
                    movement.Amount;
            }

            calculatedMovements.Add(
                new SupplierAccountMovementDto
                {
                    Id =
                        movement.Id,

                    Type =
                        movement.Type,

                    OccurredAt =
                        movement.OccurredAt,

                    Amount =
                        movement.Amount,

                    BalanceAfter =
                        runningBalance,

                    ReferenceNumber =
                        movement.ReferenceNumber,

                    Notes =
                        movement.Notes
                });
        }

        IEnumerable<SupplierAccountMovementDto>
            visibleMovements =
                calculatedMovements;

        if (fromUtc.HasValue)
        {
            visibleMovements =
                visibleMovements.Where(x =>
                    x.OccurredAt >=
                    fromUtc.Value);
        }

        var account =
            await GetAccountAsync(
                outletId);

        return new SupplierAccountLedgerDto
        {
            OutletId =
                outletId,

            TotalReceived =
                account.TotalReceived,

            TotalPaid =
                account.TotalPaid,

            CurrentDebt =
                account.CurrentDebt,

            Movements =
                visibleMovements
                    .OrderByDescending(x =>
                        x.OccurredAt)
                    .ThenByDescending(x =>
                        x.Id)
                    .ToList()
        };
    }

    private async Task EnsureOutletExistsAsync(
        int outletId)
    {
        var outletExists =
            await _context.Outlets
                .AnyAsync(x =>
                    x.Id == outletId &&
                    x.IsActive);

        if (!outletExists)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود.");
        }
    }

    private async Task<decimal> GetTreasuryBalanceAsync(
        int outletId)
    {
        var incoming =
            await _context.TreasuryMovements
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId &&
                    (x.Type ==
                        TreasuryMovementType.CashIn ||
                     x.Type ==
                        TreasuryMovementType.ShiftSettlement))
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0m;

        var outgoing =
            await _context.TreasuryMovements
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId &&
                    (x.Type ==
                        TreasuryMovementType.Expense ||
                     x.Type ==
                        TreasuryMovementType.CashOut ||
                     x.Type ==
                        TreasuryMovementType.SupplierPayment ||
                     x.Type ==
                        TreasuryMovementType.ShiftOpening))
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0m;

        return incoming -
               outgoing;
    }

    private sealed class LedgerSourceItem
    {
        public int Id { get; set; }

        public SupplierAccountMovementType Type { get; set; }

        public DateTime OccurredAt { get; set; }

        public decimal Amount { get; set; }

        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }
    }
}
