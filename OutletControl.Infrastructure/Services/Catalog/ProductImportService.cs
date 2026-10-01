using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Catalog.Import;
using OutletControl.Contracts.Catalog.Import;
using OutletControl.Domain.Entities;
using OutletControl.Infrastructure.Persistence;
using System.Globalization;

namespace OutletControl.Infrastructure.Services.Catalog;

public class ProductImportService : IProductImportService
{
    private readonly OutletControlDbContext _dbContext;

    public ProductImportService(
        OutletControlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductImportPreviewDto> PreviewAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var rows =
            await ReadAndValidateAsync(
                fileStream,
                cancellationToken);

        return new ProductImportPreviewDto
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count(x => x.IsValid),
            InvalidRows = rows.Count(x => !x.IsValid),
            Rows = rows
        };
    }

    public async Task<ProductImportResultDto> ImportAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var rows =
            await ReadAndValidateAsync(
                fileStream,
                cancellationToken);

        var result =
            new ProductImportResultDto
            {
                TotalRows = rows.Count
            };

        var invalidRows =
            rows
                .Where(x => !x.IsValid)
                .ToList();

        foreach (var row in invalidRows)
        {
            result.Errors.Add(
                new ProductImportErrorDto
                {
                    RowNumber = row.RowNumber,
                    ProductName = row.Name,
                    Message = string.Join(
                        " | ",
                        row.Errors)
                });
        }

        var validRows =
            rows
                .Where(x => x.IsValid)
                .ToList();

        if (validRows.Count == 0)
        {
            result.ImportedRows = 0;
            result.FailedRows = result.TotalRows;

            return result;
        }

        var categories =
            await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(
                    cancellationToken);

        var categoryLookup =
            categories.ToDictionary(
                x => Normalize(x.Name),
                x => x,
                StringComparer.OrdinalIgnoreCase);

        var productsToCreate =
            new List<Product>();

        foreach (var row in validRows)
        {
            var categoryKey =
                Normalize(row.CategoryName);

            if (!categoryLookup.TryGetValue(
                    categoryKey,
                    out var category))
            {
                result.Errors.Add(
                    new ProductImportErrorDto
                    {
                        RowNumber = row.RowNumber,
                        ProductName = row.Name,
                        Message =
                            $"التصنيف '{row.CategoryName}' غير موجود أو غير نشط."
                    });

                continue;
            }

            productsToCreate.Add(
                new Product
                {
                    Name = row.Name.Trim(),
                    CategoryId = category.Id,
                    SellingPrice = row.SellingPrice,
                    DisplayOrder = row.DisplayOrder,
                    IsActive = row.IsActive,
                    ImageUrl =
                        string.IsNullOrWhiteSpace(
                            row.ImageUrl)
                            ? null
                            : row.ImageUrl.Trim()
                });
        }

        if (productsToCreate.Count == 0)
        {
            result.ImportedRows = 0;
            result.FailedRows = result.TotalRows;

            return result;
        }

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        try
        {
            await _dbContext.Products
                .AddRangeAsync(
                    productsToCreate,
                    cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            result.ImportedRows =
                productsToCreate.Count;

            result.FailedRows =
                result.TotalRows -
                result.ImportedRows;

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task<List<ProductImportRowDto>>
        ReadAndValidateAsync(
            Stream fileStream,
            CancellationToken cancellationToken)
    {
        if (fileStream is null)
        {
            throw new ArgumentNullException(
                nameof(fileStream));
        }

        if (!fileStream.CanRead)
        {
            throw new InvalidOperationException(
                "تعذر قراءة ملف Excel.");
        }

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        using var workbook =
            new XLWorkbook(fileStream);

        var worksheet =
            workbook.Worksheets.FirstOrDefault();

        if (worksheet is null)
        {
            throw new InvalidOperationException(
                "ملف Excel لا يحتوي على أي Sheet.");
        }

        var lastRowUsed =
            worksheet.LastRowUsed();

        if (lastRowUsed is null)
        {
            throw new InvalidOperationException(
                "ملف Excel فارغ.");
        }

        ValidateHeaders(worksheet);

        var categories =
            await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => x.Name)
                .ToListAsync(
                    cancellationToken);

        var categoryNames =
            categories
                .Select(Normalize)
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var existingProducts =
            await _dbContext.Products
                .AsNoTracking()
                .Select(x => new
                {
                    ProductName = x.Name,
                    CategoryName = x.Category.Name
                })
                .ToListAsync(
                    cancellationToken);

        var existingProductKeys =
            existingProducts
                .Select(x =>
                    BuildProductKey(
                        x.ProductName,
                        x.CategoryName))
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var fileProductKeys =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var result =
            new List<ProductImportRowDto>();

        var lastRowNumber =
            lastRowUsed.RowNumber();

        for (var rowNumber = 2;
             rowNumber <= lastRowNumber;
             rowNumber++)
        {
            var row =
                worksheet.Row(rowNumber);

            var name =
                GetCellText(
                    row.Cell(1));

            var categoryName =
                GetCellText(
                    row.Cell(2));

            var sellingPriceText =
                GetCellText(
                    row.Cell(3));

            var displayOrderText =
                GetCellText(
                    row.Cell(4));

            var isActiveText =
                GetCellText(
                    row.Cell(5));

            var imageUrl =
                GetCellText(
                    row.Cell(6));

            if (IsEmptyRow(
                    name,
                    categoryName,
                    sellingPriceText,
                    displayOrderText,
                    isActiveText,
                    imageUrl))
            {
                continue;
            }

            var item =
                new ProductImportRowDto
                {
                    RowNumber = rowNumber,
                    Name = name,
                    CategoryName = categoryName,
                    ImageUrl =
                        string.IsNullOrWhiteSpace(
                            imageUrl)
                            ? null
                            : imageUrl
                };

            ValidateRequiredFields(item);

            ParseSellingPrice(
                sellingPriceText,
                item);

            ParseDisplayOrder(
                displayOrderText,
                item);

            ParseIsActive(
                isActiveText,
                item);

            ValidateCategory(
                item,
                categoryNames);

            ValidateDuplicates(
                item,
                existingProductKeys,
                fileProductKeys);

            item.IsValid =
                item.Errors.Count == 0;

            result.Add(item);
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException(
                "ملف Excel لا يحتوي على أي منتجات.");
        }

        return result;
    }

    private static void ValidateHeaders(
        IXLWorksheet worksheet)
    {
        var expectedHeaders =
            new[]
            {
                "Name",
                "Category",
                "SellingPrice",
                "DisplayOrder",
                "IsActive",
                "ImageUrl"
            };

        for (var column = 1;
             column <= expectedHeaders.Length;
             column++)
        {
            var actual =
                worksheet
                    .Cell(1, column)
                    .GetString()
                    .Trim();

            var expected =
                expectedHeaders[column - 1];

            if (!string.Equals(
                    actual,
                    expected,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"العمود رقم {column} يجب أن يكون اسمه '{expected}'.");
            }
        }
    }

    private static void ValidateRequiredFields(
        ProductImportRowDto item)
    {
        if (string.IsNullOrWhiteSpace(
                item.Name))
        {
            item.Errors.Add(
                "اسم المنتج مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(
                item.CategoryName))
        {
            item.Errors.Add(
                "التصنيف مطلوب.");
        }
    }

    private static void ParseSellingPrice(
        string value,
        ProductImportRowDto item)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            item.Errors.Add(
                "سعر البيع مطلوب.");

            return;
        }

        if (!TryParseDecimal(
                value,
                out var sellingPrice))
        {
            item.Errors.Add(
                "سعر البيع غير صحيح.");

            return;
        }

        if (sellingPrice < 0)
        {
            item.Errors.Add(
                "سعر البيع لا يمكن أن يكون أقل من صفر.");

            return;
        }

        item.SellingPrice =
            sellingPrice;
    }

    private static void ParseDisplayOrder(
        string value,
        ProductImportRowDto item)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            item.DisplayOrder = 0;
            return;
        }

        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var displayOrder) &&
            !int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.CurrentCulture,
                out displayOrder))
        {
            item.Errors.Add(
                "الترتيب غير صحيح.");

            return;
        }

        if (displayOrder < 0)
        {
            item.Errors.Add(
                "الترتيب لا يمكن أن يكون أقل من صفر.");

            return;
        }

        item.DisplayOrder =
            displayOrder;
    }

    private static void ParseIsActive(
        string value,
        ProductImportRowDto item)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            item.IsActive = true;
            return;
        }

        var normalized =
            value
                .Trim()
                .ToLowerInvariant();

        if (normalized is
            "true" or
            "1" or
            "yes" or
            "نعم")
        {
            item.IsActive = true;
            return;
        }

        if (normalized is
            "false" or
            "0" or
            "no" or
            "لا")
        {
            item.IsActive = false;
            return;
        }

        item.Errors.Add(
            "قيمة IsActive يجب أن تكون TRUE أو FALSE.");
    }

    private static void ValidateCategory(
        ProductImportRowDto item,
        HashSet<string> categoryNames)
    {
        if (string.IsNullOrWhiteSpace(
                item.CategoryName))
        {
            return;
        }

        var normalized =
            Normalize(
                item.CategoryName);

        if (!categoryNames.Contains(
                normalized))
        {
            item.Errors.Add(
                $"التصنيف '{item.CategoryName}' غير موجود أو غير نشط.");
        }
    }

    private static void ValidateDuplicates(
        ProductImportRowDto item,
        HashSet<string> existingProductKeys,
        HashSet<string> fileProductKeys)
    {
        if (string.IsNullOrWhiteSpace(
                item.Name) ||
            string.IsNullOrWhiteSpace(
                item.CategoryName))
        {
            return;
        }

        var key =
            BuildProductKey(
                item.Name,
                item.CategoryName);

        if (existingProductKeys.Contains(
                key))
        {
            item.Errors.Add(
                "المنتج موجود بالفعل داخل نفس التصنيف.");
        }

        if (!fileProductKeys.Add(key))
        {
            item.Errors.Add(
                "المنتج مكرر داخل ملف Excel.");
        }
    }

    private static string BuildProductKey(
        string productName,
        string categoryName)
    {
        return
            $"{Normalize(categoryName)}|{Normalize(productName)}";
    }

    private static string Normalize(
        string value)
    {
        return string.Join(
                " ",
                value
                    .Trim()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }

    private static string GetCellText(
        IXLCell cell)
    {
        return cell
            .GetFormattedString()
            .Trim();
    }

    private static bool IsEmptyRow(
        params string[] values)
    {
        return values.All(
            string.IsNullOrWhiteSpace);
    }

    private static bool TryParseDecimal(
        string value,
        out decimal result)
    {
        if (decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out result))
        {
            return true;
        }

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out result);
    }
}
