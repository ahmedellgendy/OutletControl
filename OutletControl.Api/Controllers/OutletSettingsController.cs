using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Outlets;
using OutletControl.Contracts.Outlets;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/outlet-settings")]
[Authorize(Roles = "Admin")]
public class OutletSettingsController : ControllerBase
{
    private readonly IOutletSettingsService _service;

    public OutletSettingsController(
        IOutletSettingsService service)
    {
        _service = service;
    }

    [HttpGet("current")]
    public async Task<ActionResult<OutletSettingsDto>> GetCurrent()
    {
        try
        {
            var outletId =
                GetCurrentOutletId();

            var result =
                await _service.GetAsync(
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

    [HttpPut("current")]
    public async Task<ActionResult<OutletSettingsDto>> UpdateCurrent(
        UpdateOutletSettingsRequest request)
    {
        try
        {
            var outletId =
                GetCurrentOutletId();

            var result =
                await _service.UpdateAsync(
                    outletId,
                    request);

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

    private int GetCurrentOutletId()
    {
        var value =
            User.FindFirstValue(
                "outlet_id");

        if (!int.TryParse(
            value,
            out var outletId))
        {
            throw new InvalidOperationException(
                "تعذر تحديد المنفذ المرتبط بالمستخدم.");
        }

        return outletId;
    }
}
