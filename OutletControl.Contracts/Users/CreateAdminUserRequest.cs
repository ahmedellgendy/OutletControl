namespace OutletControl.Contracts.Users;

public class CreateAdminUserRequest
{
    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int OutletId { get; set; }
}
