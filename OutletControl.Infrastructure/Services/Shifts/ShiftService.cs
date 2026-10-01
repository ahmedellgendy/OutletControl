using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Shifts;
using OutletControl.Contracts.Shifts;
using OutletControl.Domain.Entities;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Shifts;

public class ShiftService : IShiftService
{
    private readonly OutletControlDbContext _context;

    public ShiftService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<ShiftDto> OpenShiftAsync(
        OpenShiftRequest request)
    {
        if (request.OutletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        if (request.OpeningCash < 0)
        {
            throw new InvalidOperationException(
                "رصيد بداية اليوم لا يمكن أن يكون أقل من صفر.");
        }

        var outletExists =
            await _context.Outlets
                .AnyAsync(x =>
                    x.Id == request.OutletId &&
                    x.IsActive);

        if (!outletExists)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود أو غير نشط.");
        }

        var hasOpenShift =
            await _context.Shifts
                .AnyAsync(x =>
                    x.OutletId == request.OutletId &&
                    !x.IsClosed);

        if (hasOpenShift)
        {
            throw new InvalidOperationException(
                "يوجد يوم مفتوح بالفعل لهذا المنفذ.");
        }

        if (request.OpeningCash > 0)
        {
            var treasuryBalance =
                await GetTreasuryBalanceAsync(
                    request.OutletId);

            if (request.OpeningCash >
                treasuryBalance)
            {
                throw new InvalidOperationException(
                    $"رصيد الخزنة غير كافٍ. الرصيد الحالي {treasuryBalance:0.##} ج.");
            }
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var shift =
                new Shift
                {
                    OutletId =
                        request.OutletId,

                    OpenedAt =
                        DateTime.UtcNow,

                    OpeningCash =
                        request.OpeningCash,

                    SalesAmount =
                        0,

                    ExpectedCash =
                        request.OpeningCash,

                    ActualCash =
                        null,

                    CashDifference =
                        null,

                    ClosedAt =
                        null,

                    Notes =
                        null,

                    IsClosed =
                        false
                };

            _context.Shifts.Add(
                shift);

            await _context.SaveChangesAsync();

            if (request.OpeningCash > 0)
            {
                _context.TreasuryMovements.Add(
                    new TreasuryMovement
                    {
                        OutletId =
                            request.OutletId,

                        ShiftId =
                            shift.Id,

                        Type =
                            TreasuryMovementType.ShiftOpening,

                        Amount =
                            request.OpeningCash,

                        OccurredAt =
                            shift.OpenedAt,

                        Category =
                            "تمويل درج الكاشير",

                        Notes =
                            $"رصيد بداية الشيفت #{shift.Id}",

                        ReferenceType =
                            nameof(Shift),

                        ReferenceId =
                            shift.Id
                    });

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return Map(
                shift);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ShiftDto?> GetOpenShiftAsync(
        int outletId)
    {
        var shift =
            await _context.Shifts
                .FirstOrDefaultAsync(x =>
                    x.OutletId == outletId &&
                    !x.IsClosed);

        if (shift is null)
            return null;

        var salesAmount =
            await GetShiftSalesAmountAsync(
                shift.Id);

        shift.SalesAmount =
            salesAmount;

        shift.ExpectedCash =
            shift.OpeningCash +
            salesAmount;

        await _context.SaveChangesAsync();

        return Map(
            shift);
    }

    public async Task<ShiftDto> CloseShiftAsync(
        CloseShiftRequest request)
    {
        if (request.ShiftId <= 0)
        {
            throw new InvalidOperationException(
                "الوردية غير صحيحة.");
        }

        if (request.ActualCash < 0)
        {
            throw new InvalidOperationException(
                "الكاش الفعلي لا يمكن أن يكون أقل من صفر.");
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var shift =
                await _context.Shifts
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ShiftId &&
                        !x.IsClosed);

            if (shift is null)
            {
                throw new InvalidOperationException(
                    "الوردية غير موجودة أو تم إغلاقها بالفعل.");
            }

            var salesAmount =
                await GetShiftSalesAmountAsync(
                    shift.Id);

            shift.SalesAmount =
                salesAmount;

            shift.ExpectedCash =
                shift.OpeningCash +
                salesAmount;

            shift.ActualCash =
                request.ActualCash;

            shift.CashDifference =
                request.ActualCash -
                shift.ExpectedCash;

            shift.Notes =
                string.IsNullOrWhiteSpace(
                    request.Notes)
                    ? null
                    : request.Notes.Trim();

            shift.ClosedAt =
                DateTime.UtcNow;

            shift.IsClosed =
                true;

            // Legacy protection:
            // if this shift was opened before Treasury/ShiftOpening integration,
            // create the missing opening transfer before settlement.
            if (shift.OpeningCash > 0)
            {
                var hasOpeningMovement =
                    await _context.TreasuryMovements
                        .AnyAsync(x =>
                            x.ShiftId == shift.Id &&
                            x.Type ==
                                TreasuryMovementType.ShiftOpening);

                if (!hasOpeningMovement)
                {
                    _context.TreasuryMovements.Add(
                        new TreasuryMovement
                        {
                            OutletId =
                                shift.OutletId,

                            ShiftId =
                                shift.Id,

                            Type =
                                TreasuryMovementType.ShiftOpening,

                            Amount =
                                shift.OpeningCash,

                            OccurredAt =
                                shift.OpenedAt,

                            Category =
                                "تمويل درج الكاشير",

                            Notes =
                                $"رصيد بداية الشيفت #{shift.Id}",

                            ReferenceType =
                                nameof(Shift),

                            ReferenceId =
                                shift.Id
                        });
                }
            }

            var hasSettlementMovement =
                await _context.TreasuryMovements
                    .AnyAsync(x =>
                        x.ShiftId == shift.Id &&
                        x.Type ==
                            TreasuryMovementType.ShiftSettlement);

            if (!hasSettlementMovement &&
                request.ActualCash > 0)
            {
                _context.TreasuryMovements.Add(
                    new TreasuryMovement
                    {
                        OutletId =
                            shift.OutletId,

                        ShiftId =
                            shift.Id,

                        Type =
                            TreasuryMovementType.ShiftSettlement,

                        Amount =
                            request.ActualCash,

                        OccurredAt =
                            shift.ClosedAt.Value,

                        Category =
                            "تسوية الشيفت",

                        Notes =
                            $"استلام إغلاق الشيفت #{shift.Id}",

                        ReferenceType =
                            nameof(Shift),

                        ReferenceId =
                            shift.Id
                    });
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Map(
                shift);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<ShiftHistoryItemDto>> GetHistoryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null,
        bool? isClosed = null)
    {
        if (outletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        var query =
            _context.Shifts
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId);

        if (fromUtc.HasValue)
        {
            query =
                query.Where(x =>
                    x.OpenedAt >=
                    fromUtc.Value);
        }

        if (toUtcExclusive.HasValue)
        {
            query =
                query.Where(x =>
                    x.OpenedAt <
                    toUtcExclusive.Value);
        }

        if (isClosed.HasValue)
        {
            query =
                query.Where(x =>
                    x.IsClosed ==
                    isClosed.Value);
        }

        return await query
            .OrderByDescending(x =>
                x.OpenedAt)
            .Select(x =>
                new ShiftHistoryItemDto
                {
                    Id =
                        x.Id,

                    OutletId =
                        x.OutletId,

                    OutletName =
                        x.Outlet.Name,

                    OpenedAt =
                        x.OpenedAt,

                    ClosedAt =
                        x.ClosedAt,

                    OpeningCash =
                        x.OpeningCash,

                    SalesAmount =
                        x.IsClosed
                            ? x.SalesAmount
                            : x.Sales.Sum(s =>
                                (decimal?)s.TotalAmount)
                              ?? 0m,

                    ExpectedCash =
                        x.IsClosed
                            ? x.ExpectedCash
                            : x.OpeningCash +
                              (x.Sales.Sum(s =>
                                  (decimal?)s.TotalAmount)
                               ?? 0m),

                    ActualCash =
                        x.ActualCash,

                    CashDifference =
                        x.CashDifference,

                    Notes =
                        x.Notes,

                    IsClosed =
                        x.IsClosed
                })
            .ToListAsync();
    }

    private async Task<decimal> GetShiftSalesAmountAsync(
        int shiftId)
    {
        return await _context.Sales
            .Where(x =>
                x.ShiftId == shiftId)
            .SumAsync(x =>
                (decimal?)x.TotalAmount)
            ?? 0m;
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

    private static ShiftDto Map(
        Shift shift)
    {
        return new ShiftDto
        {
            Id =
                shift.Id,

            OutletId =
                shift.OutletId,

            OpenedAt =
                shift.OpenedAt,

            ClosedAt =
                shift.ClosedAt,

            OpeningCash =
                shift.OpeningCash,

            SalesAmount =
                shift.SalesAmount,

            ExpectedCash =
                shift.ExpectedCash,

            ActualCash =
                shift.ActualCash,

            CashDifference =
                shift.CashDifference,

            Notes =
                shift.Notes,

            IsClosed =
                shift.IsClosed
        };
    }
}
