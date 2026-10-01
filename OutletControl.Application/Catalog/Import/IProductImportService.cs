using OutletControl.Contracts.Catalog.Import;

namespace OutletControl.Application.Catalog.Import;

public interface IProductImportService
{
    Task<ProductImportPreviewDto> PreviewAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);

    Task<ProductImportResultDto> ImportAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);
}