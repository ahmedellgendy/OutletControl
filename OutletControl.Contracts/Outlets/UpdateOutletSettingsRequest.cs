namespace OutletControl.Contracts.Outlets;

public class UpdateOutletSettingsRequest
{
    public string Name { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? TaxNumber { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string? LogoUrl { get; set; }
}
