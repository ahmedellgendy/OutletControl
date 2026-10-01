using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize(Roles = "Admin")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public UploadsController(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("product-image")]
    public async Task<IActionResult> UploadProductImage(
        IFormFile file)
    {
        if (file is null ||
            file.Length == 0)
        {
            return BadRequest(new
            {
                message =
                    "اختر صورة."
            });
        }

        var allowedExtensions =
            new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

        var extension =
            Path.GetExtension(
                    file.FileName)
                .ToLowerInvariant();

        if (!allowedExtensions.Contains(
            extension))
        {
            return BadRequest(new
            {
                message =
                    "نوع الصورة غير مدعوم."
            });
        }

        const long maxFileSize =
            20 * 1024 * 1024;

        if (file.Length >
            maxFileSize)
        {
            return BadRequest(new
            {
                message =
                    "حجم الصورة يجب ألا يزيد عن 20 MB."
            });
        }

        var webRoot =
            _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(
            webRoot))
        {
            webRoot =
                Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");
        }

        var uploadFolder =
            Path.Combine(
                webRoot,
                "uploads",
                "products");

        Directory.CreateDirectory(
            uploadFolder);

        var fileName =
            $"{Guid.NewGuid():N}{extension}";

        var filePath =
            Path.Combine(
                uploadFolder,
                fileName);

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        await file.CopyToAsync(
            stream);

        var imageUrl =
            $"{Request.Scheme}://{Request.Host}/uploads/products/{fileName}";

        return Ok(new
        {
            imageUrl
        });
    }
}