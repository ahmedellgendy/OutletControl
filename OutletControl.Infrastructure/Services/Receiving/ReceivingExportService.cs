using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Receiving;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Receiving;

public class ReceivingExportService :
    IReceivingExportService
{
    private readonly OutletControlDbContext _context;

    public ReceivingExportService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportReceiptAsync(
        int outletId,
        int receiptId)
    {
        if (outletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        if (receiptId <= 0)
        {
            throw new InvalidOperationException(
                "فاتورة الاستلام غير صحيحة.");
        }

        var receipt =
            await _context.StockReceipts
                .AsNoTracking()
                .Where(x =>
                    x.Id == receiptId &&
                    x.OutletId == outletId)
                .Select(x => new
                {
                    x.Id,
                    x.ReceivedAt,
                    x.ReferenceNumber,
                    x.Notes,
                    x.TotalAmount,

                    OutletName =
                        x.Outlet.Name,

                    Items =
                        x.Items
                            .OrderBy(i =>
                                i.Product.Name)
                            .Select(i => new
                            {
                                ProductName =
                                    i.Product.Name,

                                i.Quantity,
                                i.UnitCost,

                                TotalCost =
                                    i.Quantity *
                                    i.UnitCost
                            })
                            .ToList()
                })
                .FirstOrDefaultAsync();

        if (receipt is null)
        {
            throw new InvalidOperationException(
                "فاتورة الاستلام غير موجودة.");
        }

        using var workbook =
            new XLWorkbook();

        var worksheet =
            workbook.Worksheets.Add(
                $"استلام {receipt.Id}");

        worksheet.RightToLeft =
            true;

        var row = 1;

        // ==============================
        // Title
        // ==============================

        worksheet.Range(
                row,
                1,
                row,
                5)
            .Merge();

        worksheet.Cell(
                row,
                1)
            .Value =
            $"فاتورة استلام رقم #{receipt.Id}";

        worksheet.Cell(
                row,
                1)
            .Style.Font.Bold =
            true;

        worksheet.Cell(
                row,
                1)
            .Style.Font.FontSize =
            18;

        worksheet.Cell(
                row,
                1)
            .Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Cell(
                row,
                1)
            .Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Range(
                row,
                1,
                row,
                5)
            .Style.Fill.BackgroundColor =
            XLColor.FromHtml(
                "#FDE9E7");

        worksheet.Row(row)
            .Height =
            30;

        row += 2;

        // ==============================
        // Receipt information
        // ==============================

        var localReceivedAt =
            ConvertToEgyptTime(
                receipt.ReceivedAt);

        worksheet.Cell(
                row,
                1)
            .Value =
            "رقم الاستلام";

        worksheet.Cell(
                row,
                2)
            .Value =
            receipt.Id;

        worksheet.Cell(
                row,
                4)
            .Value =
            "التاريخ";

        worksheet.Cell(
                row,
                5)
            .Value =
            localReceivedAt;

        worksheet.Cell(
                row,
                5)
            .Style.DateFormat.Format =
            "dd/MM/yyyy hh:mm AM/PM";

        row++;

        worksheet.Cell(
                row,
                1)
            .Value =
            "المنفذ";

        worksheet.Cell(
                row,
                2)
            .Value =
            receipt.OutletName;

        worksheet.Cell(
                row,
                4)
            .Value =
            "رقم المرجع";

        worksheet.Cell(
                row,
                5)
            .Value =
            string.IsNullOrWhiteSpace(
                receipt.ReferenceNumber)
                ? "بدون رقم مرجع"
                : receipt.ReferenceNumber;

        row++;

        worksheet.Cell(
                row,
                1)
            .Value =
            "الملاحظات";

        worksheet.Range(
                row,
                2,
                row,
                5)
            .Merge();

        worksheet.Cell(
                row,
                2)
            .Value =
            string.IsNullOrWhiteSpace(
                receipt.Notes)
                ? "—"
                : receipt.Notes;

        var infoRange =
            worksheet.Range(
                3,
                1,
                row,
                5);

        infoRange.Style
            .Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        infoRange.Style.Border
            .OutsideBorder =
            XLBorderStyleValues.Thin;

        infoRange.Style.Border
            .InsideBorder =
            XLBorderStyleValues.Thin;

        worksheet.Range(
                3,
                1,
                row,
                1)
            .Style.Font.Bold =
            true;

        worksheet.Range(
                3,
                4,
                row,
                4)
            .Style.Font.Bold =
            true;

        row += 2;

        // ==============================
        // Items header
        // ==============================

        var tableHeaderRow =
            row;

        worksheet.Cell(
                row,
                1)
            .Value =
            "م";

        worksheet.Cell(
                row,
                2)
            .Value =
            "المنتج";

        worksheet.Cell(
                row,
                3)
            .Value =
            "الكمية";

        worksheet.Cell(
                row,
                4)
            .Value =
            "تكلفة الوحدة";

        worksheet.Cell(
                row,
                5)
            .Value =
            "إجمالي الصنف";

        var headerRange =
            worksheet.Range(
                row,
                1,
                row,
                5);

        headerRange.Style.Font.Bold =
            true;

        headerRange.Style.Fill
            .BackgroundColor =
            XLColor.FromHtml(
                "#E32B21");

        headerRange.Style.Font
            .FontColor =
            XLColor.White;

        headerRange.Style.Alignment
            .Horizontal =
            XLAlignmentHorizontalValues.Center;

        headerRange.Style.Alignment
            .Vertical =
            XLAlignmentVerticalValues.Center;

        row++;

        // ==============================
        // Items
        // ==============================

        var index =
            1;

        foreach (var item in receipt.Items)
        {
            worksheet.Cell(
                    row,
                    1)
                .Value =
                index;

            worksheet.Cell(
                    row,
                    2)
                .Value =
                item.ProductName;

            worksheet.Cell(
                    row,
                    3)
                .Value =
                item.Quantity;

            worksheet.Cell(
                    row,
                    4)
                .Value =
                item.UnitCost;

            worksheet.Cell(
                    row,
                    5)
                .Value =
                item.TotalCost;

            worksheet.Cell(
                    row,
                    4)
                .Style.NumberFormat.Format =
                "#,##0.00";

            worksheet.Cell(
                    row,
                    5)
                .Style.NumberFormat.Format =
                "#,##0.00";

            index++;
            row++;
        }

        var lastItemRow =
            row - 1;

        if (lastItemRow >=
            tableHeaderRow)
        {
            var tableRange =
                worksheet.Range(
                    tableHeaderRow,
                    1,
                    lastItemRow,
                    5);

            tableRange.Style.Border
                .OutsideBorder =
                XLBorderStyleValues.Thin;

            tableRange.Style.Border
                .InsideBorder =
                XLBorderStyleValues.Thin;

            tableRange.Style.Alignment
                .Vertical =
                XLAlignmentVerticalValues.Center;

            worksheet.Range(
                    tableHeaderRow + 1,
                    1,
                    lastItemRow,
                    1)
                .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            worksheet.Range(
                    tableHeaderRow + 1,
                    3,
                    lastItemRow,
                    5)
                .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
        }

        row++;

        // ==============================
        // Totals
        // ==============================

        var totalQuantity =
            receipt.Items.Sum(x =>
                x.Quantity);

        worksheet.Range(
                row,
                1,
                row,
                3)
            .Merge();

        worksheet.Cell(
                row,
                1)
            .Value =
            "إجمالي الكمية";

        worksheet.Cell(
                row,
                4)
            .Value =
            totalQuantity;

        worksheet.Cell(
                row,
                5)
            .Value =
            "قطعة";

        worksheet.Range(
                row,
                1,
                row,
                5)
            .Style.Font.Bold =
            true;

        worksheet.Range(
                row,
                1,
                row,
                5)
            .Style.Fill.BackgroundColor =
            XLColor.FromHtml(
                "#F8F9FA");

        worksheet.Range(
                row,
                1,
                row,
                5)
            .Style.Border.OutsideBorder =
            XLBorderStyleValues.Thin;

        row++;

        worksheet.Range(
                row,
                1,
                row,
                3)
            .Merge();

        worksheet.Cell(
                row,
                1)
            .Value =
            "إجمالي الفاتورة";

        worksheet.Range(
                row,
                4,
                row,
                5)
            .Merge();

        worksheet.Cell(
                row,
                4)
            .Value =
            receipt.TotalAmount;

        worksheet.Cell(
                row,
                4)
            .Style.NumberFormat.Format =
            "#,##0.00 \"ج\"";

        var totalRange =
            worksheet.Range(
                row,
                1,
                row,
                5);

        totalRange.Style.Font.Bold =
            true;

        totalRange.Style.Font.FontSize =
            14;

        totalRange.Style.Fill
            .BackgroundColor =
            XLColor.FromHtml(
                "#FFF2CC");

        totalRange.Style.Border
            .OutsideBorder =
            XLBorderStyleValues.Thin;

        totalRange.Style.Alignment
            .Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Row(row)
            .Height =
            28;

        // ==============================
        // Final formatting
        // ==============================

        worksheet.Column(1)
            .Width =
            14;

        worksheet.Column(2)
            .Width =
            35;

        worksheet.Column(3)
            .Width =
            15;

        worksheet.Column(4)
            .Width =
            20;

        worksheet.Column(5)
            .Width =
            24;

        worksheet.SheetView
            .FreezeRows(
                tableHeaderRow);

        worksheet.PageSetup
            .PageOrientation =
            XLPageOrientation.Portrait;

        worksheet.PageSetup
            .FitToPages(
                1,
                0);

        using var stream =
            new MemoryStream();

        workbook.SaveAs(
            stream);

        return stream.ToArray();
    }

    private static DateTime ConvertToEgyptTime(
        DateTime utcDateTime)
    {
        var utc =
            DateTime.SpecifyKind(
                utcDateTime,
                DateTimeKind.Utc);

        TimeZoneInfo timeZone;

        try
        {
            timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    "Egypt Standard Time");
        }
        catch
        {
            timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    "Africa/Cairo");
        }

        return TimeZoneInfo
            .ConvertTimeFromUtc(
                utc,
                timeZone);
    }
}