using OutletControl.Contracts.Catalog;

namespace OutletControl.Application.Interfaces.Catalog;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);

    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request);
}