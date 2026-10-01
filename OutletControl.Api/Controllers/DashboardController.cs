using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Dashboard;
using OutletControl.Contracts.Dashboard;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Owner,Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("owner/{outletId:int}")]
    public async Task<ActionResult<OwnerDashboardDto>> GetOwnerDashboard(
        int outletId)
    {
        try
        {
            var dashboard =
                await _dashboardService.GetOwnerDashboardAsync(outletId);

            return Ok(dashboard);
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