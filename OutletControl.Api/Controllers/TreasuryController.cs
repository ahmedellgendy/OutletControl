using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Treasury;
using OutletControl.Contracts.Treasury;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/treasury")]
[Authorize(Roles = "Admin,Owner")]
public class TreasuryController : ControllerBase
{
    private readonly ITreasuryService _treasuryService;

    public TreasuryController(
        ITreasuryService treasuryService)
    {
        _treasuryService =
            treasuryService;
    }

    [HttpGet("{outletId:int}")]
    public async Task<ActionResult<TreasuryPageDto>> GetTreasury(
        int outletId,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtcExclusive = null)
    {
        try
        {
            var result =
                await _treasuryService
                    .GetTreasuryAsync(
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

    [HttpPost("movements")]
    public async Task<ActionResult<TreasuryMovementDto>> CreateMovement(
        CreateTreasuryMovementRequest request)
    {
        try
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdValue,
                out var userId))
            {
                return Unauthorized(new
                {
                    message =
                        "تعذر تحديد المستخدم الحالي."
                });
            }

            var result =
                await _treasuryService
                    .CreateMovementAsync(
                        request,
                        userId);

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