using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Inventory;
using OutletControl.Contracts.Inventory;
using OutletControl.Domain.Entities;
using OutletControl.Domain.Enums;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly OutletControlDbContext _context;

    public InventoryService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task SetOpeningStockAsync(
        SetOpeningStockRequest request)
    {
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

        var requestedItems =
            request.Items
                .Where(x => x.Quantity > 0)
                .GroupBy(x => x.ProductId)
                .Select(x => new OpeningStockItemRequest
                {
                    ProductId =
                        x.Key,

                    Quantity =
                        x.Sum(i => i.Quantity)
                })
                .ToList();

        if (requestedItems.Count == 0)
        {
            throw new InvalidOperationException(
                "يجب إدخال كمية لمنتج واحد على الأقل.");
        }

        var productIds =
            requestedItems
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

        var activeProductIds =
            await _context.Products
                .AsNoTracking()
                .Where(x =>
                    productIds.Contains(x.Id) &&
                    x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();

        var missingProductIds =
            productIds
                .Except(activeProductIds)
                .ToList();

        if (missingProductIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"بعض المنتجات غير موجودة أو غير نشطة: {string.Join(", ", missingProductIds)}.");
        }

        var productsWithOpeningBalance =
            await _context.StockMovements
                .AsNoTracking()
                .Where(x =>
                    x.OutletId ==
                        request.OutletId &&
                    productIds.Contains(
                        x.ProductId) &&
                    x.Type ==
                        StockMovementType.OpeningBalance)
                .Select(x =>
                    x.ProductId)
                .Distinct()
                .ToListAsync();

        var itemsToSave =
            requestedItems
                .Where(x =>
                    !productsWithOpeningBalance
                        .Contains(
                            x.ProductId))
                .ToList();

        if (itemsToSave.Count == 0)
        {
            throw new InvalidOperationException(
                "تم تسجيل الرصيد الافتتاحي بالفعل لكل المنتجات المحددة.");
        }

        var itemsToSaveIds =
            itemsToSave
                .Select(x => x.ProductId)
                .ToList();

        var productsWithOperationalMovements =
            await _context.StockMovements
                .AsNoTracking()
                .Where(x =>
                    x.OutletId ==
                        request.OutletId &&
                    itemsToSaveIds.Contains(
                        x.ProductId) &&
                    x.Type !=
                        StockMovementType.OpeningBalance)
                .Select(x =>
                    x.ProductId)
                .Distinct()
                .ToListAsync();

        if (productsWithOperationalMovements.Count > 0)
        {
            throw new InvalidOperationException(
                $"لا يمكن تسجيل رصيد افتتاحي بعد وجود حركات مخزون للمنتجات: {string.Join(", ", productsWithOperationalMovements)}.");
        }

        var existingBalances =
            await _context.StockBalances
                .Where(x =>
                    x.OutletId ==
                        request.OutletId &&
                    itemsToSaveIds.Contains(
                        x.ProductId))
                .ToDictionaryAsync(
                    x => x.ProductId);

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            foreach (var item in itemsToSave)
            {
                if (!existingBalances.TryGetValue(
                    item.ProductId,
                    out var balance))
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
                                0m
                        };

                    _context.StockBalances
                        .Add(balance);
                }
                else
                {
                    balance.Quantity =
                        item.Quantity;

                    balance.AverageUnitCost =
                        0m;
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
                            StockMovementType.OpeningBalance,

                        Notes =
                            "الرصيد الافتتاحي"
                    });
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<StockBalanceDto>>
        GetStockAsync(
            int outletId,
            bool includeFinancialData)
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

        return await _context.StockBalances
            .AsNoTracking()
            .Where(x =>
                x.OutletId == outletId)
            .OrderBy(x =>
                x.Product.Category.DisplayOrder)
            .ThenBy(x =>
                x.Product.DisplayOrder)
            .Select(x =>
                new StockBalanceDto
                {
                    ProductId =
                        x.ProductId,

                    ProductName =
                        x.Product.Name,

                    CategoryName =
                        x.Product.Category.Name,

                    ImageUrl =
                        x.Product.ImageUrl,

                    Quantity =
                        x.Quantity,

                    SellingPrice =
                        x.Product.SellingPrice,

                    AverageUnitCost =
                     includeFinancialData
                         ? x.AverageUnitCost
                         : 0m
                })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<OpeningStockProductDto>>
        GetOpeningStockProductsAsync(
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

        return await _context.Products
            .AsNoTracking()
            .Where(product =>
                product.IsActive &&
                !_context.StockMovements.Any(
                    movement =>
                        movement.OutletId ==
                            outletId &&
                        movement.ProductId ==
                            product.Id &&
                        movement.Type ==
                            StockMovementType.OpeningBalance))
            .OrderBy(x =>
                x.Category.DisplayOrder)
            .ThenBy(x =>
                x.DisplayOrder)
            .Select(x =>
                new OpeningStockProductDto
                {
                    ProductId =
                        x.Id,

                    ProductName =
                        x.Name,

                    CategoryName =
                        x.Category.Name,

                    ImageUrl =
                        x.ImageUrl,

                    DisplayOrder =
                        x.DisplayOrder
                })
            .ToListAsync();
    }
}