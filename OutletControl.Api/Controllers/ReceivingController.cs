using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Receiving;
using OutletControl.Contracts.Receiving;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/receiving")]
[Authorize(Roles = "Admin,Owner,Cashier")]
public class ReceivingController :
    ControllerBase
{
    private readonly IReceivingService
        _receivingService;

    private readonly IReceivingExportService
        _receivingExportService;

    public ReceivingController(
        IReceivingService receivingService,
        IReceivingExportService receivingExportService)
    {
        _receivingService =
            receivingService;

        _receivingExportService =
            receivingExportService;
    }

    [HttpPost]
    public async Task<ActionResult<StockReceiptDto>>
        Receive(
            ReceiveStockRequest request)
    {
        try
        {
            var result =
                await _receivingService
                    .ReceiveAsync(
                        request);

            return Ok(
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    [HttpGet("history/{outletId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<
        ActionResult<
            IReadOnlyList<
                StockReceiptHistoryItemDto>>>
        GetHistory(
            int outletId,
            [FromQuery] DateTime? fromUtc = null,
            [FromQuery] DateTime? toUtcExclusive = null)
    {
        try
        {
            var result =
                await _receivingService
                    .GetHistoryAsync(
                        outletId,
                        fromUtc,
                        toUtcExclusive);

            return Ok(
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    [HttpGet(
        "export/{outletId:int}/{receiptId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult>
        ExportReceipt(
            int outletId,
            int receiptId)
    {
        try
        {
            var file =
                await _receivingExportService
                    .ExportReceiptAsync(
                        outletId,
                        receiptId);

            var fileName =
                $"Receiving-{receiptId}.xlsx";

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }
}