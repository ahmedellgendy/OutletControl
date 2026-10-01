using OutletControl.Contracts.Catalog;

namespace OutletControl.Application.Interfaces.Catalog;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync();

    Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(int categoryId);

    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductRequest request);

    Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request);
}