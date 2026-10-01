using Microsoft.AspNetCore.Identity;

namespace OutletControl.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public int OutletId { get; set; }

    public bool IsActive { get; set; } = true;
}