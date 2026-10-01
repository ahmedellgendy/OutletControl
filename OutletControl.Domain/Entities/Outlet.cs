namespace OutletControl.Domain.Entities;

public class Outlet
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? TaxNumber { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
