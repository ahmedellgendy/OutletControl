using OutletControl.Contracts.Users;

namespace OutletControl.Application.Interfaces.Users;

public interface IUserManagementService
{
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync();

    Task<AdminUserDto> CreateUserAsync(
        CreateAdminUserRequest request);

    Task<AdminUserDto> UpdateUserAsync(
        int userId,
        UpdateAdminUserRequest request);

    Task<AdminUserDto> SetActiveAsync(
        int userId,
        bool isActive,
        int currentAdminUserId);

    Task ResetPasswordAsync(
        int userId,
        string newPassword);
}
