using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Catalog;
using OutletControl.Contracts.Catalog;
using OutletControl.Domain.Entities;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Catalog;

public class CategoryService : ICategoryService
{
    private readonly OutletControlDbContext _context;

    public CategoryService(OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder
        };
    }

    public async Task<CategoryDto?> UpdateAsync(
        int id,
        UpdateCategoryRequest request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category is null)
            return null;

        category.Name = request.Name.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder
        };
    }
}