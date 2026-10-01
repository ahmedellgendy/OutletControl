using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Sales;
using OutletControl.Contracts.Sales;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Owner,Cashier")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(
        ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(
        CreateSaleRequest request)
    {
        try
        {
            var sale =
                await _salesService
                    .CreateSaleAsync(
                        request);

            return Ok(sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("history/{outletId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<IReadOnlyList<SaleHistoryItemDto>>> GetHistory(
        int outletId,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtcExclusive = null)
    {
        try
        {
            var result =
                await _salesService
                    .GetHistoryAsync(
                        outletId,
                        fromUtc,
                        toUtcExclusive);

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
}
