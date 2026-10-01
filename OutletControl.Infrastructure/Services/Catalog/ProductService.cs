using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Catalog;
using OutletControl.Contracts.Catalog;
using OutletControl.Domain.Entities;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Catalog;

public class ProductService : IProductService
{
    private readonly OutletControlDbContext _context;

    public ProductService(OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(x => x.Category.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                SellingPrice = x.SellingPrice,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(
        int categoryId)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(x =>
                x.CategoryId == categoryId &&
                x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                SellingPrice = x.SellingPrice,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                SellingPrice = x.SellingPrice,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(x => x.Id == request.CategoryId);

        if (!categoryExists)
            throw new InvalidOperationException("التصنيف غير موجود.");

        var product = new Product
        {
            Name = request.Name.Trim(),
            SellingPrice = request.SellingPrice,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(product.Id))!;
    }

    public async Task<ProductDto?> UpdateAsync(
        int id,
        UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
            return null;

        var categoryExists = await _context.Categories
            .AnyAsync(x => x.Id == request.CategoryId);

        if (!categoryExists)
            throw new InvalidOperationException("التصنيف غير موجود.");

        product.Name = request.Name.Trim();
        product.SellingPrice = request.SellingPrice;
        product.ImageUrl = request.ImageUrl;
        product.DisplayOrder = request.DisplayOrder;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }
}