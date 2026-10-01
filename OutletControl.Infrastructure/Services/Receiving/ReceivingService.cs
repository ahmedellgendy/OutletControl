using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Common.Stock;
using OutletControl.Application.Interfaces.Receiving;
using OutletControl.Contracts.Receiving;
using OutletControl.Domain.Entities;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Receiving;

public class ReceivingService : IReceivingService
{
    private readonly OutletControlDbContext _context;

    public ReceivingService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<StockReceiptDto> ReceiveAsync(
        ReceiveStockRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "يجب إضافة منتج واحد على الأقل.");
        }

        if (string.IsNullOrWhiteSpace(
            request.ClientReferenceId))
        {
            throw new InvalidOperationException(
                "معرف عملية الاستلام غير موجود.");
        }

        var clientReferenceId =
            request.ClientReferenceId.Trim();

        if (clientReferenceId.Length > 64)
        {
            throw new InvalidOperationException(
                "معرف عملية الاستلام غير صحيح.");
        }

        var outletExists =
            await _context.Outlets
                .AnyAsync(x =>
                    x.Id == request.OutletId &&
                    x.IsActive);

        if (!outletExists)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود.");
        }

        // Idempotency:
        // لو نفس العملية اتسجلت قبل كده نرجعها
        // بدل إنشاء استلام جديد.
        var existingReceipt =
            await GetReceiptByClientReferenceAsync(
                request.OutletId,
                clientReferenceId);

        if (existingReceipt is not null)
        {
            return existingReceipt;
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var receipt =
                new StockReceipt
                {
                    OutletId =
                        request.OutletId,

                    ClientReferenceId =
                        clientReferenceId,

                    ReferenceNumber =
                        string.IsNullOrWhiteSpace(
                            request.ReferenceNumber)
                            ? null
                            : request.ReferenceNumber.Trim(),

                    Notes =
                        string.IsNullOrWhiteSpace(
                            request.Notes)
                            ? null
                            : request.Notes.Trim(),

                    ReceivedAt =
                        DateTime.UtcNow
                };

            decimal totalAmount = 0m;

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "كمية الاستلام يجب أن تكون أكبر من صفر.");
                }

                if (item.UnitCost < 0)
                {
                    throw new InvalidOperationException(
                        "تكلفة المنتج لا يمكن أن تكون أقل من صفر.");
                }

                var productExists =
                    await _context.Products
                        .AnyAsync(x =>
                            x.Id == item.ProductId &&
                            x.IsActive);

                if (!productExists)
                {
                    throw new InvalidOperationException(
                        $"المنتج رقم {item.ProductId} غير موجود.");
                }

                var receiptItem =
                    new StockReceiptItem
                    {
                        ProductId =
                            item.ProductId,

                        Quantity =
                            item.Quantity,

                        UnitCost =
                            item.UnitCost
                    };

                receipt.Items.Add(
                    receiptItem);

                totalAmount +=
                    item.Quantity *
                    item.UnitCost;

                var balance =
                    await _context.StockBalances
                        .FirstOrDefaultAsync(x =>
                            x.OutletId == request.OutletId &&
                            x.ProductId == item.ProductId);

                if (balance is null)
                {
                    balance =
                        new StockBalance
                        {
                            OutletId =
                                request.OutletId,

                            ProductId =
                                item.ProductId,

                            Quantity =
                                item.Quantity,

                            AverageUnitCost =
                                item.UnitCost
                        };

                    _context.StockBalances.Add(
                        balance);
                }
                else
                {
                    var newAverageUnitCost =
                        StockCostCalculator
                            .CalculateNewAverageCost(
                                balance.Quantity,
                                balance.AverageUnitCost,
                                item.Quantity,
                                item.UnitCost);

                    balance.AverageUnitCost =
                        newAverageUnitCost;

                    balance.Quantity +=
                        item.Quantity;
                }

                _context.StockMovements.Add(
                    new StockMovement
                    {
                        OutletId =
                            request.OutletId,

                        ProductId =
                            item.ProductId,

                        Quantity =
                            item.Quantity,

                        Type =
                            StockMovementType.Receiving,

                        Notes =
                            string.IsNullOrWhiteSpace(
                                request.ReferenceNumber)

                                ? "استلام بضاعة"

                                : $"استلام بضاعة - مرجع: {request.ReferenceNumber.Trim()}"
                    });
            }

            receipt.TotalAmount =
                totalAmount;

            _context.StockReceipts.Add(
                receipt);

            await _context.SaveChangesAsync();

            var result =
                new StockReceiptDto
                {
                    Id =
                        receipt.Id,

                    OutletId =
                        receipt.OutletId,

                    ReceivedAt =
                        receipt.ReceivedAt,

                    ReferenceNumber =
                        receipt.ReferenceNumber,

                    TotalAmount =
                        receipt.TotalAmount
                };

            await transaction.CommitAsync();

            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "تم تعديل رصيد أحد المنتجات أثناء عملية الاستلام. حدّث البيانات وحاول مرة أخرى.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            _context.ChangeTracker.Clear();

            // ممكن يكون السبب إن نفس ClientReferenceId
            // اتسجل في Request متزامن ونجح بالفعل.
            var existingAfterConflict =
                await GetReceiptByClientReferenceAsync(
                    request.OutletId,
                    clientReferenceId);

            if (existingAfterConflict is not null)
            {
                return existingAfterConflict;
            }

            throw new InvalidOperationException(
                "حدث تعارض أثناء تحديث المخزون. حاول تنفيذ عملية الاستلام مرة أخرى.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<StockReceiptHistoryItemDto>>
        GetHistoryAsync(
            int outletId,
            DateTime? fromUtc = null,
            DateTime? toUtcExclusive = null)
    {
        if (outletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        var outletExists =
            await _context.Outlets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == outletId);

        if (!outletExists)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود.");
        }

        var query =
            _context.StockReceipts
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId);

        if (fromUtc.HasValue)
        {
            query =
                query.Where(x =>
                    x.ReceivedAt >=
                    fromUtc.Value);
        }

        if (toUtcExclusive.HasValue)
        {
            query =
                query.Where(x =>
                    x.ReceivedAt <
                    toUtcExclusive.Value);
        }

        return await query
            .OrderByDescending(x =>
                x.ReceivedAt)
            .Select(x =>
                new StockReceiptHistoryItemDto
                {
                    Id =
                        x.Id,

                    OutletId =
                        x.OutletId,

                    OutletName =
                        x.Outlet.Name,

                    ReceivedAt =
                        x.ReceivedAt,

                    ReferenceNumber =
                        x.ReferenceNumber,

                    Notes =
                        x.Notes,

                    TotalAmount =
                        x.TotalAmount,

                    ProductsCount =
                        x.Items.Count,

                    TotalQuantity =
                        x.Items.Sum(i =>
                            i.Quantity),

                    Items =
                        x.Items
                            .OrderBy(i =>
                                i.Product.Name)
                            .Select(i =>
                                new StockReceiptHistoryLineDto
                                {
                                    ProductId =
                                        i.ProductId,

                                    ProductName =
                                        i.Product.Name,

                                    Quantity =
                                        i.Quantity,

                                    UnitCost =
                                        i.UnitCost,

                                    TotalCost =
                                        i.Quantity *
                                        i.UnitCost
                                })
                            .ToList()
                })
            .ToListAsync();
    }

    private async Task<StockReceiptDto?>
        GetReceiptByClientReferenceAsync(
            int outletId,
            string clientReferenceId)
    {
        return await _context.StockReceipts
            .AsNoTracking()
            .Where(x =>
                x.OutletId == outletId &&
                x.ClientReferenceId ==
                    clientReferenceId)
            .Select(x =>
                new StockReceiptDto
                {
                    Id =
                        x.Id,

                    OutletId =
                        x.OutletId,

                    ReceivedAt =
                        x.ReceivedAt,

                    ReferenceNumber =
                        x.ReferenceNumber,

                    TotalAmount =
                        x.TotalAmount
                })
            .FirstOrDefaultAsync();
    }
}