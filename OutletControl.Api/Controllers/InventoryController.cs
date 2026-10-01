using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Inventory;
using OutletControl.Contracts.Inventory;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Cashier,Owner,Admin")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(
        IInventoryService inventoryService)
    {
        _inventoryService =
            inventoryService;
    }

    [HttpGet("{outletId:int}")]
    public async Task<ActionResult<IReadOnlyList<StockBalanceDto>>>
        GetStock(
            int outletId)
    {
        try
        {
            var includeFinancialData =
                User.IsInRole("Admin") ||
                User.IsInRole("Owner");

            var result =
                await _inventoryService
                    .GetStockAsync(
                        outletId,
                        includeFinancialData);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("opening-stock/products/{outletId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<OpeningStockProductDto>>>
        GetOpeningStockProducts(
            int outletId)
    {
        try
        {
            var result =
                await _inventoryService
                    .GetOpeningStockProductsAsync(
                        outletId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("opening-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetOpeningStock(
        SetOpeningStockRequest request)
    {
        try
        {
            await _inventoryService
                .SetOpeningStockAsync(
                    request);

            return Ok(new
            {
                message =
                    "تم حفظ الرصيد الافتتاحي بنجاح."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}