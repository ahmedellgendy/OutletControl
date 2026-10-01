using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Users;
using OutletControl.Contracts.Users;
using OutletControl.Infrastructure.Identity;

namespace OutletControl.Infrastructure.Services.Users;

public class UserManagementService : IUserManagementService
{
    private static readonly string[] AllowedManagedRoles =
    [
        "Owner",
        "Cashier"
    ];

    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync()
    {
        var users =
            await _userManager.Users
                .OrderBy(x => x.UserName)
                .ToListAsync();

        var result =
            new List<AdminUserDto>(
                users.Count);

        foreach (var user in users)
        {
            result.Add(
                await MapAsync(user));
        }

        return result;
    }

    public async Task<AdminUserDto> CreateUserAsync(
        CreateAdminUserRequest request)
    {
        var userName =
            request.UserName?.Trim();

        var fullName =
            request.FullName?.Trim();

        var role =
            NormalizeRole(
                request.Role);

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException(
                "اسم المستخدم مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException(
                "اسم المستخدم الكامل مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException(
                "كلمة المرور مطلوبة.");
        }

        if (request.OutletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        var existing =
            await _userManager.FindByNameAsync(
                userName);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                "اسم المستخدم مستخدم بالفعل.");
        }

        var user =
            new ApplicationUser
            {
                UserName =
                    userName,

                FullName =
                    fullName,

                OutletId =
                    request.OutletId,

                IsActive =
                    true
            };

        var createResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                JoinErrors(
                    createResult));
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(
                user);

            throw new InvalidOperationException(
                JoinErrors(
                    roleResult));
        }

        return await MapAsync(
            user);
    }

    public async Task<AdminUserDto> UpdateUserAsync(
        int userId,
        UpdateAdminUserRequest request)
    {
        var user =
            await GetUserAsync(
                userId);

        await EnsureNotProtectedAdminAsync(
            user);

        if (string.IsNullOrWhiteSpace(
            request.FullName))
        {
            throw new InvalidOperationException(
                "الاسم الكامل مطلوب.");
        }

        if (request.OutletId <= 0)
        {
            throw new InvalidOperationException(
                "المنفذ غير صحيح.");
        }

        var role =
            NormalizeRole(
                request.Role);

        user.FullName =
            request.FullName.Trim();

        user.OutletId =
            request.OutletId;

        var updateResult =
            await _userManager.UpdateAsync(
                user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                JoinErrors(
                    updateResult));
        }

        var currentRoles =
            await _userManager.GetRolesAsync(
                user);

        foreach (var currentRole in currentRoles)
        {
            if (!string.Equals(
                    currentRole,
                    role,
                    StringComparison.OrdinalIgnoreCase))
            {
                var removeResult =
                    await _userManager.RemoveFromRoleAsync(
                        user,
                        currentRole);

                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        JoinErrors(
                            removeResult));
                }
            }
        }

        if (!await _userManager.IsInRoleAsync(
            user,
            role))
        {
            var addRoleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    role);

            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    JoinErrors(
                        addRoleResult));
            }
        }

        return await MapAsync(
            user);
    }

    public async Task<AdminUserDto> SetActiveAsync(
        int userId,
        bool isActive,
        int currentAdminUserId)
    {
        var user =
            await GetUserAsync(
                userId);

        if (user.Id ==
            currentAdminUserId)
        {
            throw new InvalidOperationException(
                "لا يمكن للمسؤول تعطيل حسابه الحالي.");
        }

        await EnsureNotProtectedAdminAsync(
            user);

        user.IsActive =
            isActive;

        var result =
            await _userManager.UpdateAsync(
                user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                JoinErrors(
                    result));
        }

        return await MapAsync(
            user);
    }

    public async Task ResetPasswordAsync(
        int userId,
        string newPassword)
    {
        var user =
            await GetUserAsync(
                userId);

        await EnsureNotProtectedAdminAsync(
            user);

        if (string.IsNullOrWhiteSpace(
            newPassword))
        {
            throw new InvalidOperationException(
                "كلمة المرور الجديدة مطلوبة.");
        }

        var token =
            await _userManager
                .GeneratePasswordResetTokenAsync(
                    user);

        var result =
            await _userManager
                .ResetPasswordAsync(
                    user,
                    token,
                    newPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                JoinErrors(
                    result));
        }
    }

    private async Task<ApplicationUser> GetUserAsync(
        int userId)
    {
        var user =
            await _userManager.FindByIdAsync(
                userId.ToString());

        return user
            ?? throw new InvalidOperationException(
                "المستخدم غير موجود.");
    }

    private async Task EnsureNotProtectedAdminAsync(
        ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(
            user,
            "Admin"))
        {
            throw new InvalidOperationException(
                "حساب مسؤول النظام محمي ولا يمكن تعديله من هذه الشاشة.");
        }
    }

    private static string NormalizeRole(
        string? role)
    {
        var normalized =
            AllowedManagedRoles
                .FirstOrDefault(x =>
                    string.Equals(
                        x,
                        role?.Trim(),
                        StringComparison.OrdinalIgnoreCase));

        return normalized
            ?? throw new InvalidOperationException(
                "الدور يجب أن يكون Owner أو Cashier.");
    }

    private async Task<AdminUserDto> MapAsync(
        ApplicationUser user)
    {
        var roles =
            await _userManager.GetRolesAsync(
                user);

        var role =
            roles.FirstOrDefault()
            ?? string.Empty;

        return new AdminUserDto
        {
            Id =
                user.Id,

            UserName =
                user.UserName
                ?? string.Empty,

            FullName =
                user.FullName,

            OutletId =
                user.OutletId,

            IsActive =
                user.IsActive,

            Role =
                role,

            IsProtectedAdmin =
                string.Equals(
                    role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string JoinErrors(
        IdentityResult result)
    {
        return string.Join(
            "، ",
            result.Errors
                .Select(x =>
                    x.Description));
    }
}
