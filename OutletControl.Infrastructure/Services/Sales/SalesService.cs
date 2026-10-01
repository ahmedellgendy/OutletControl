using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Sales;
using OutletControl.Contracts.Sales;
using OutletControl.Domain.Entities;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Sales;

public class SalesService : ISalesService
{
    private readonly OutletControlDbContext _context;

    public SalesService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<SaleDto> CreateSaleAsync(
        CreateSaleRequest request)
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
                "معرف عملية البيع غير موجود.");
        }

        var clientReferenceId =
            request.ClientReferenceId.Trim();

        if (clientReferenceId.Length > 64)
        {
            throw new InvalidOperationException(
                "معرف عملية البيع غير صحيح.");
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

        var existingSale =
            await GetSaleByClientReferenceAsync(
                request.OutletId,
                clientReferenceId);

        if (existingSale is not null)
        {
            return existingSale;
        }

        var shift =
            await _context.Shifts
                .FirstOrDefaultAsync(x =>
                    x.OutletId == request.OutletId &&
                    !x.IsClosed);

        if (shift is null)
        {
            throw new InvalidOperationException(
                "يجب فتح اليوم قبل تسجيل أي عملية بيع.");
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var sale =
                new Sale
                {
                    OutletId =
                        request.OutletId,

                    ShiftId =
                        shift.Id,

                    ClientReferenceId =
                        clientReferenceId,

                    SoldAt =
                        DateTime.UtcNow
                };

            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "كمية البيع يجب أن تكون أكبر من صفر.");
                }

                var product =
                    await _context.Products
                        .FirstOrDefaultAsync(x =>
                            x.Id == item.ProductId &&
                            x.IsActive);

                if (product is null)
                {
                    throw new InvalidOperationException(
                        $"المنتج رقم {item.ProductId} غير موجود.");
                }

                var balance =
                    await _context.StockBalances
                        .FirstOrDefaultAsync(x =>
                            x.OutletId == request.OutletId &&
                            x.ProductId == item.ProductId);

                if (balance is null)
                {
                    throw new InvalidOperationException(
                        $"لا يوجد مخزون للمنتج {product.Name}.");
                }

                if (balance.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"الكمية المتاحة من {product.Name} غير كافية.");
                }

                var itemTotal =
                    product.SellingPrice *
                    item.Quantity;

                var saleItem =
                    new SaleItem
                    {
                        ProductId =
                            product.Id,

                        Quantity =
                            item.Quantity,

                        UnitPrice =
                            product.SellingPrice,

                        UnitCost =
                            balance.AverageUnitCost,

                        TotalAmount =
                            itemTotal
                    };

                sale.Items.Add(
                    saleItem);

                balance.Quantity -=
                    item.Quantity;

                _context.StockMovements.Add(
                    new StockMovement
                    {
                        OutletId =
                            request.OutletId,

                        ProductId =
                            product.Id,

                        Quantity =
                            -item.Quantity,

                        Type =
                            StockMovementType.Sale,

                        Notes =
                            "بيع"
                    });

                totalAmount +=
                    itemTotal;
            }

            sale.TotalAmount =
                totalAmount;

            shift.SalesAmount +=
                totalAmount;

            shift.ExpectedCash =
                shift.OpeningCash +
                shift.SalesAmount;

            _context.Sales.Add(
                sale);

            await _context.SaveChangesAsync();

            var saleDto =
                await _context.Sales
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == sale.Id)
                    .Select(x =>
                        new SaleDto
                        {
                            Id =
                                x.Id,

                            OutletId =
                                x.OutletId,

                            SoldAt =
                                x.SoldAt,

                            TotalAmount =
                                x.TotalAmount,

                            Items =
                                x.Items
                                    .Select(i =>
                                        new SaleItemDto
                                        {
                                            ProductId =
                                                i.ProductId,

                                            ProductName =
                                                i.Product.Name,

                                            Quantity =
                                                i.Quantity,

                                            UnitPrice =
                                                i.UnitPrice,

                                            Total =
                                                i.TotalAmount
                                        })
                                    .ToList()
                        })
                    .FirstAsync();

            await transaction.CommitAsync();

            return saleDto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "تم تعديل رصيد أحد المنتجات أثناء عملية البيع. حدّث الصفحة وحاول مرة أخرى.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            _context.ChangeTracker.Clear();

            var existingAfterConflict =
                await GetSaleByClientReferenceAsync(
                    request.OutletId,
                    clientReferenceId);

            if (existingAfterConflict is not null)
            {
                return existingAfterConflict;
            }

            throw new InvalidOperationException(
                "حدث تعارض أثناء تسجيل البيع. حاول مرة أخرى.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<SaleHistoryItemDto>> GetHistoryAsync(
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
            _context.Sales
                .AsNoTracking()
                .Where(x =>
                    x.OutletId == outletId);

        if (fromUtc.HasValue)
        {
            query =
                query.Where(x =>
                    x.SoldAt >= fromUtc.Value);
        }

        if (toUtcExclusive.HasValue)
        {
            query =
                query.Where(x =>
                    x.SoldAt < toUtcExclusive.Value);
        }

        return await query
            .OrderByDescending(x =>
                x.SoldAt)
            .Select(x =>
                new SaleHistoryItemDto
                {
                    Id =
                        x.Id,

                    OutletId =
                        x.OutletId,

                    OutletName =
                        x.Outlet.Name,

                    ShiftId =
                        x.ShiftId,

                    SoldAt =
                        x.SoldAt,

                    TotalAmount =
                        x.TotalAmount,

                    TotalCost =
                        x.Items.Sum(i =>
                            i.UnitCost *
                            i.Quantity),

                    GrossProfit =
                        x.TotalAmount -
                        x.Items.Sum(i =>
                            i.UnitCost *
                            i.Quantity),

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
                                new SaleHistoryLineDto
                                {
                                    ProductId =
                                        i.ProductId,

                                    ProductName =
                                        i.Product.Name,

                                    Quantity =
                                        i.Quantity,

                                    UnitPrice =
                                        i.UnitPrice,

                                    UnitCost =
                                        i.UnitCost,

                                    TotalAmount =
                                        i.TotalAmount,

                                    TotalCost =
                                        i.UnitCost *
                                        i.Quantity,

                                    GrossProfit =
                                        i.TotalAmount -
                                        (i.UnitCost *
                                         i.Quantity)
                                })
                            .ToList()
                })
            .ToListAsync();
    }

    private async Task<SaleDto?> GetSaleByClientReferenceAsync(
        int outletId,
        string clientReferenceId)
    {
        return await _context.Sales
            .AsNoTracking()
            .Where(x =>
                x.OutletId == outletId &&
                x.ClientReferenceId ==
                    clientReferenceId)
            .Select(x =>
                new SaleDto
                {
                    Id =
                        x.Id,

                    OutletId =
                        x.OutletId,

                    SoldAt =
                        x.SoldAt,

                    TotalAmount =
                        x.TotalAmount,

                    Items =
                        x.Items
                            .Select(i =>
                                new SaleItemDto
                                {
                                    ProductId =
                                        i.ProductId,

                                    ProductName =
                                        i.Product.Name,

                                    Quantity =
                                        i.Quantity,

                                    UnitPrice =
                                        i.UnitPrice,

                                    Total =
                                        i.TotalAmount
                                })
                            .ToList()
                })
            .FirstOrDefaultAsync();
    }
}
