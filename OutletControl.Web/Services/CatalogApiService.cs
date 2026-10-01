using OutletControl.Contracts.Catalog;
using OutletControl.Contracts.Catalog.Import;
using System.Net.Http.Json;

namespace OutletControl.Web.Services;

public class CatalogApiService
{
    private readonly HttpClient _httpClient;

    public CatalogApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _httpClient
            .GetFromJsonAsync<List<CategoryDto>>(
                "api/categories")
            ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto?> CreateCategoryAsync(
        CreateCategoryRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/categories",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر حفظ التصنيف: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<CategoryDto>();
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/categories/{id}",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر تعديل التصنيف: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<CategoryDto>();
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        return await _httpClient
            .GetFromJsonAsync<List<ProductDto>>(
                "api/products")
            ?? new List<ProductDto>();
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(
        int categoryId)
    {
        return await _httpClient
            .GetFromJsonAsync<List<ProductDto>>(
                $"api/products/category/{categoryId}")
            ?? new List<ProductDto>();
    }

    public async Task<ProductDto?> CreateProductAsync(
        CreateProductRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/products",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر حفظ المنتج: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<ProductDto>();
    }

    public async Task<ProductDto?> UpdateProductAsync(
        int id,
        UpdateProductRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/products/{id}",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر تعديل المنتج: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<ProductDto>();
    }

    public async Task<ProductImportPreviewDto?> PreviewProductsImportAsync(
    Stream fileStream,
    string fileName,
    string contentType)
    {
        using var content =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                contentType);

        content.Add(
            fileContent,
            "file",
            fileName);

        var response =
            await _httpClient.PostAsync(
                "api/products/import/preview",
                content);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر معاينة ملف المنتجات: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<ProductImportPreviewDto>();
    }

    public async Task<ProductImportResultDto?> ImportProductsAsync(
        Stream fileStream,
        string fileName,
        string contentType)
    {
        using var content =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                contentType);

        content.Add(
            fileContent,
            "file",
            fileName);

        var response =
            await _httpClient.PostAsync(
                "api/products/import",
                content);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر استيراد المنتجات: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<ProductImportResultDto>();
    }
}