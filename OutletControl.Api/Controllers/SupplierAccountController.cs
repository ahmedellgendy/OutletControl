using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.SupplierAccount;
using OutletControl.Contracts.SupplierAccount;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/supplier-account")]
[Authorize(Roles = "Admin,Owner")]
public class SupplierAccountController : ControllerBase
{
    private readonly ISupplierAccountService _supplierAccountService;

    public SupplierAccountController(
        ISupplierAccountService supplierAccountService)
    {
        _supplierAccountService =
            supplierAccountService;
    }

    [HttpGet("{outletId:int}")]
    public async Task<ActionResult<SupplierAccountDto>> GetAccount(
        int outletId)
    {
        try
        {
            var result =
                await _supplierAccountService
                    .GetAccountAsync(
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

    [HttpGet("{outletId:int}/ledger")]
    public async Task<ActionResult<SupplierAccountLedgerDto>> GetLedger(
        int outletId,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtcExclusive = null)
    {
        try
        {
            var result =
                await _supplierAccountService
                    .GetLedgerAsync(
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

    [HttpPost("payments")]
    public async Task<ActionResult<SupplierAccountDto>> AddPayment(
        CreateSupplierPaymentRequest request)
    {
        try
        {
            var result =
                await _supplierAccountService
                    .AddPaymentAsync(
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
}
