using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Users;
using OutletControl.Contracts.Users;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _service;

    public UsersController(
        IUserManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers()
    {
        var result =
            await _service.GetUsersAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> CreateUser(
        CreateAdminUserRequest request)
    {
        try
        {
            var result =
                await _service.CreateUserAsync(
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

    [HttpPut("{userId:int}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(
        int userId,
        UpdateAdminUserRequest request)
    {
        try
        {
            var result =
                await _service.UpdateUserAsync(
                    userId,
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

    [HttpPut("{userId:int}/active")]
    public async Task<ActionResult<AdminUserDto>> SetActive(
        int userId,
        SetUserActiveRequest request)
    {
        try
        {
            var currentUserId =
                GetCurrentUserId();

            var result =
                await _service.SetActiveAsync(
                    userId,
                    request.IsActive,
                    currentUserId);

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

    [HttpPost("{userId:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        int userId,
        ResetUserPasswordRequest request)
    {
        try
        {
            await _service.ResetPasswordAsync(
                userId,
                request.NewPassword);

            return Ok(new
            {
                message =
                    "تم تغيير كلمة المرور بنجاح."
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

    private int GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
            value,
            out var userId))
        {
            throw new InvalidOperationException(
                "تعذر تحديد المستخدم الحالي.");
        }

        return userId;
    }
}
