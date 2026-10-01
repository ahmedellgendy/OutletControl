using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Catalog.Import;
using OutletControl.Contracts.Catalog.Import;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/products/import")]
[Authorize(Roles = "Owner")]
public class ProductImportController : ControllerBase
{
    private readonly IProductImportService _productImportService;

    public ProductImportController(
        IProductImportService productImportService)
    {
        _productImportService = productImportService;
    }

    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductImportPreviewDto>>
        PreviewAsync(
            IFormFile file,
            CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(
                "اختر ملف Excel صالح.");
        }

        if (!IsExcelFile(file))
        {
            return BadRequest(
                "الملف يجب أن يكون بصيغة XLSX.");
        }

        try
        {
            await using var stream =
                file.OpenReadStream();

            var result =
                await _productImportService
                    .PreviewAsync(
                        stream,
                        cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductImportResultDto>>
        ImportAsync(
            IFormFile file,
            CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(
                "اختر ملف Excel صالح.");
        }

        if (!IsExcelFile(file))
        {
            return BadRequest(
                "الملف يجب أن يكون بصيغة XLSX.");
        }

        try
        {
            await using var stream =
                file.OpenReadStream();

            var result =
                await _productImportService
                    .ImportAsync(
                        stream,
                        cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static bool IsExcelFile(
        IFormFile file)
    {
        var extension =
            Path.GetExtension(file.FileName);

        return string.Equals(
            extension,
            ".xlsx",
            StringComparison.OrdinalIgnoreCase);
    }
}