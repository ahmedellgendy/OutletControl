namespace OutletControl.Contracts.Users;

public class ResetUserPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
