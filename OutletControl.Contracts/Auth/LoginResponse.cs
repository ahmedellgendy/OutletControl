namespace OutletControl.Contracts.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int OutletId { get; set; }
}