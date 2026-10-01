namespace OutletControl.Contracts.Users;

public class AdminUserDto
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int OutletId { get; set; }

    public bool IsActive { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool IsProtectedAdmin { get; set; }
}
