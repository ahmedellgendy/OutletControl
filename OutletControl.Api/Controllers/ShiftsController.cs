using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Shifts;
using OutletControl.Contracts.Shifts;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize(Roles = "Admin,Owner,Cashier")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(
        IShiftService shiftService)
    {
        _shiftService =
            shiftService;
    }

    [HttpPost("open")]
    public async Task<ActionResult<ShiftDto>> Open(
        OpenShiftRequest request)
    {
        try
        {
            var shift =
                await _shiftService
                    .OpenShiftAsync(
                        request);

            return Ok(shift);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("open/{outletId:int}")]
    public async Task<ActionResult<ShiftDto>> GetOpenShift(
        int outletId)
    {
        try
        {
            var shift =
                await _shiftService
                    .GetOpenShiftAsync(
                        outletId);

            if (shift is null)
            {
                return NotFound(new
                {
                    message =
                        "لا يوجد شيفت مفتوح حاليًا."
                });
            }

            return Ok(shift);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("close")]
    public async Task<ActionResult<ShiftDto>> Close(
        CloseShiftRequest request)
    {
        try
        {
            var shift =
                await _shiftService
                    .CloseShiftAsync(
                        request);

            return Ok(shift);
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
    public async Task<ActionResult<IReadOnlyList<ShiftHistoryItemDto>>> GetHistory(
        int outletId,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtcExclusive = null,
        [FromQuery] bool? isClosed = null)
    {
        try
        {
            var items =
                await _shiftService
                    .GetHistoryAsync(
                        outletId,
                        fromUtc,
                        toUtcExclusive,
                        isClosed);

            return Ok(items);
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