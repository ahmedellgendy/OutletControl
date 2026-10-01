namespace OutletControl.Contracts.Users;

public class UpdateAdminUserRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int OutletId { get; set; }
}
