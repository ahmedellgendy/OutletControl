using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Treasury;
using OutletControl.Contracts.Treasury;
using OutletControl.Domain.Entities;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Treasury;

public class TreasuryService : ITreasuryService
{
    private readonly OutletControlDbContext _context;

    public TreasuryService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<TreasuryMovementDto> CreateMovementAsync(
        CreateTreasuryMovementRequest request,
        int? createdByUserId)
    {
        if (request.OutletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "قيمة الحركة يجب أن تكون أكبر من صفر.");
        }

        if (request.Type is
            TreasuryMovementTypeDto.SupplierPayment or
            TreasuryMovementTypeDto.ShiftOpening or
            TreasuryMovementTypeDto.ShiftSettlement)
        {
            throw new InvalidOperationException(
                "هذا النوع من الحركات يتم تسجيله تلقائيًا من النظام.");
        }

        if (request.Type is not
            (TreasuryMovementTypeDto.Expense
             or TreasuryMovementTypeDto.CashIn
             or TreasuryMovementTypeDto.CashOut))
        {
            throw new InvalidOperationException(
                "نوع حركة الخزنة غير صحيح.");
        }

        await EnsureOutletExistsAsync(
            request.OutletId);

        if (request.Type is
            TreasuryMovementTypeDto.Expense or
            TreasuryMovementTypeDto.CashOut)
        {
            var currentBalance =
                await GetBalanceAsync(
                    request.OutletId);

            if (request.Amount > currentBalance)
            {
                throw new InvalidOperationException(
                    "رصيد الخزنة غير كافٍ لتنفيذ الحركة.");
            }
        }

        var movement =
            new TreasuryMovement
            {
                OutletId =
                    request.OutletId,

                ShiftId =
                    null,

                Type =
                    MapType(request.Type),

                Amount =
                    request.Amount,

                OccurredAt =
                    DateTime.UtcNow,

                Category =
                    request.Type ==
                        TreasuryMovementTypeDto.Expense &&
                    !string.IsNullOrWhiteSpace(
                        request.Category)
                        ? request.Category.Trim()
                        : null,

                Notes =
                    string.IsNullOrWhiteSpace(
                        request.Notes)
                        ? null
                        : request.Notes.Trim(),

                CreatedByUserId =
                    createdByUserId
            };

        _context.TreasuryMovements.Add(
            movement);

        await _context.SaveChangesAsync();

        return MapMovement(
            movement);
    }

    public async Task<TreasuryPageDto> GetTreasuryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null)
    {
        await EnsureOutletExistsAsync(
            outletId);

        var allMovements =
            _context.TreasuryMovements
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId);

        var totals =
            await allMovements
                .GroupBy(_ => 1)
                .Select(g =>
                    new
                    {
                        CashIn =
                            g.Where(x =>
                                x.Type ==
                                TreasuryMovementType.CashIn)
                             .Sum(x =>
                                (decimal?)x.Amount)
                            ?? 0m,

                        ShiftSettlements =
                            g.Where(x =>
                                x.Type ==
                                TreasuryMovementType.ShiftSettlement)
                             .Sum(x =>
                                (decimal?)x.Amount)
                            ?? 0m,

                        Expenses =
                            g.Where(x =>
                                x.Type ==
                                TreasuryMovementType.Expense)
                             .Sum(x =>
                                (decimal?)x.Amount)
                            ?? 0m,

                        CashOut =
                            g.Where(x =>
                                x.Type ==
                                TreasuryMovementType.CashOut)
                             .Sum(x =>
                                (decimal?)x.Amount)
                            ?? 0m,

                        SupplierPayments =
                            g.Where(x =>
                                x.Type ==
                                TreasuryMovementType.SupplierPayment)
                             .Sum(x =>
                                (decimal?)x.Amount)
                            ?? 0m,

                        ShiftOpenings =
                            g.Where(x =>
                                x.Type ==
                                TreasuryMovementType.ShiftOpening)
                             .Sum(x =>
                                (decimal?)x.Amount)
                            ?? 0m
                    })
                .FirstOrDefaultAsync();

        var summary =
            new TreasurySummaryDto
            {
                OutletId =
                    outletId,

                TotalCashIn =
                    totals?.CashIn ?? 0m,

                TotalShiftSettlements =
                    totals?.ShiftSettlements ?? 0m,

                TotalExpenses =
                    totals?.Expenses ?? 0m,

                TotalCashOut =
                    totals?.CashOut ?? 0m,

                TotalSupplierPayments =
                    totals?.SupplierPayments ?? 0m,

                TotalShiftOpenings =
                    totals?.ShiftOpenings ?? 0m
            };

        summary.CurrentBalance =
            summary.TotalCashIn +
            summary.TotalShiftSettlements -
            summary.TotalExpenses -
            summary.TotalCashOut -
            summary.TotalSupplierPayments -
            summary.TotalShiftOpenings;

        var movementsQuery =
            allMovements;

        if (fromUtc.HasValue)
        {
            movementsQuery =
                movementsQuery.Where(x =>
                    x.OccurredAt >=
                    fromUtc.Value);
        }

        if (toUtcExclusive.HasValue)
        {
            movementsQuery =
                movementsQuery.Where(x =>
                    x.OccurredAt <
                    toUtcExclusive.Value);
        }

        var movements =
            await movementsQuery
                .OrderByDescending(x =>
                    x.OccurredAt)
                .ThenByDescending(x =>
                    x.Id)
                .Select(x =>
                    new TreasuryMovementDto
                    {
                        Id =
                            x.Id,

                        OutletId =
                            x.OutletId,

                        ShiftId =
                            x.ShiftId,

                        Type =
                            (TreasuryMovementTypeDto)
                            (int)x.Type,

                        Amount =
                            x.Amount,

                        OccurredAt =
                            x.OccurredAt,

                        Category =
                            x.Category,

                        Notes =
                            x.Notes,

                        CreatedByUserId =
                            x.CreatedByUserId,

                        ReferenceType =
                            x.ReferenceType,

                        ReferenceId =
                            x.ReferenceId
                    })
                .ToListAsync();

        return new TreasuryPageDto
        {
            Summary =
                summary,

            Movements =
                movements
        };
    }

    public async Task<decimal> GetBalanceAsync(
        int outletId)
    {
        await EnsureOutletExistsAsync(
            outletId);

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

    private async Task EnsureOutletExistsAsync(
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
    }

    private static TreasuryMovementType MapType(
        TreasuryMovementTypeDto type)
    {
        return type switch
        {
            TreasuryMovementTypeDto.Expense =>
                TreasuryMovementType.Expense,

            TreasuryMovementTypeDto.CashIn =>
                TreasuryMovementType.CashIn,

            TreasuryMovementTypeDto.CashOut =>
                TreasuryMovementType.CashOut,

            TreasuryMovementTypeDto.SupplierPayment =>
                TreasuryMovementType.SupplierPayment,

            TreasuryMovementTypeDto.ShiftOpening =>
                TreasuryMovementType.ShiftOpening,

            TreasuryMovementTypeDto.ShiftSettlement =>
                TreasuryMovementType.ShiftSettlement,

            _ => throw new InvalidOperationException(
                "نوع حركة الخزنة غير صحيح.")
        };
    }

    private static TreasuryMovementDto MapMovement(
        TreasuryMovement movement)
    {
        return new TreasuryMovementDto
        {
            Id =
                movement.Id,

            OutletId =
                movement.OutletId,

            ShiftId =
                movement.ShiftId,

            Type =
                (TreasuryMovementTypeDto)
                (int)movement.Type,

            Amount =
                movement.Amount,

            OccurredAt =
                movement.OccurredAt,

            Category =
                movement.Category,

            Notes =
                movement.Notes,

            CreatedByUserId =
                movement.CreatedByUserId,

            ReferenceType =
                movement.ReferenceType,

            ReferenceId =
                movement.ReferenceId
        };
    }
}
