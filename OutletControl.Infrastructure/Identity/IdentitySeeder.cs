using Microsoft.AspNetCore.Identity;

namespace OutletControl.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        const string adminRole = "Admin";
        const string ownerRole = "Owner";
        const string cashierRole = "Cashier";

        await EnsureRoleAsync(
            roleManager,
            adminRole);

        await EnsureRoleAsync(
            roleManager,
            ownerRole);

        await EnsureRoleAsync(
            roleManager,
            cashierRole);

        await EnsureUserAsync(
            userManager,
            userName: "admin",
            fullName: "مسؤول النظام",
            password: "123456",
            role: adminRole);

        await EnsureUserAsync(
            userManager,
            userName: "owner",
            fullName: "صاحب المنفذ",
            password: "123456",
            role: ownerRole);

        await EnsureUserAsync(
            userManager,
            userName: "cashier",
            fullName: "عامل المنفذ",
            password: "123456",
            role: cashierRole);
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole<int>> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result =
            await roleManager.CreateAsync(
                new IdentityRole<int>(roleName));

        if (!result.Succeeded)
        {
            var errors =
                string.Join(
                    ", ",
                    result.Errors
                        .Select(x => x.Description));

            throw new InvalidOperationException(
                $"تعذر إنشاء الدور {roleName}: {errors}");
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string fullName,
        string password,
        string role)
    {
        var user =
            await userManager.FindByNameAsync(
                userName);

        if (user is null)
        {
            user =
                new ApplicationUser
                {
                    UserName = userName,
                    FullName = fullName,
                    OutletId = 1,
                    IsActive = true
                };

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            if (!createResult.Succeeded)
            {
                var errors =
                    string.Join(
                        ", ",
                        createResult.Errors
                            .Select(x => x.Description));

                throw new InvalidOperationException(
                    $"تعذر إنشاء المستخدم {userName}: {errors}");
            }
        }
        else
        {
            var changed = false;

            if (user.FullName != fullName)
            {
                user.FullName = fullName;
                changed = true;
            }

            if (user.OutletId != 1)
            {
                user.OutletId = 1;
                changed = true;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (changed)
            {
                var updateResult =
                    await userManager.UpdateAsync(
                        user);

                if (!updateResult.Succeeded)
                {
                    var errors =
                        string.Join(
                            ", ",
                            updateResult.Errors
                                .Select(x => x.Description));

                    throw new InvalidOperationException(
                        $"تعذر تحديث المستخدم {userName}: {errors}");
                }
            }
        }

        var currentRoles =
            await userManager.GetRolesAsync(
                user);

        foreach (var currentRole in currentRoles)
        {
            if (!string.Equals(
                    currentRole,
                    role,
                    StringComparison.OrdinalIgnoreCase))
            {
                await userManager.RemoveFromRoleAsync(
                    user,
                    currentRole);
            }
        }

        if (!await userManager.IsInRoleAsync(
                user,
                role))
        {
            var addRoleResult =
                await userManager.AddToRoleAsync(
                    user,
                    role);

            if (!addRoleResult.Succeeded)
            {
                var errors =
                    string.Join(
                        ", ",
                        addRoleResult.Errors
                            .Select(x => x.Description));

                throw new InvalidOperationException(
                    $"تعذر إضافة المستخدم {userName} إلى الدور {role}: {errors}");
            }
        }
    }
}