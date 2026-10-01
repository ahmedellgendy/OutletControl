using Microsoft.EntityFrameworkCore;
using OutletControl.Application.Interfaces.Outlets;
using OutletControl.Contracts.Outlets;
using OutletControl.Infrastructure.Persistence;

namespace OutletControl.Infrastructure.Services.Outlets;

public class OutletSettingsService : IOutletSettingsService
{
    private readonly OutletControlDbContext _context;

    public OutletSettingsService(
        OutletControlDbContext context)
    {
        _context = context;
    }

    public async Task<OutletSettingsDto> GetAsync(
        int outletId)
    {
        var outlet =
            await _context.Outlets
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == outletId);

        if (outlet is null)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود.");
        }

        return Map(outlet);
    }

    public async Task<OutletSettingsDto> UpdateAsync(
        int outletId,
        UpdateOutletSettingsRequest request)
    {
        if (string.IsNullOrWhiteSpace(
            request.Name))
        {
            throw new InvalidOperationException(
                "اسم المنفذ مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(
            request.CurrencyCode))
        {
            throw new InvalidOperationException(
                "العملة مطلوبة.");
        }

        var currencyCode =
            request.CurrencyCode
                .Trim()
                .ToUpperInvariant();

        if (currencyCode.Length != 3)
        {
            throw new InvalidOperationException(
                "كود العملة يجب أن يتكون من 3 أحرف مثل EGP.");
        }

        var outlet =
            await _context.Outlets
                .FirstOrDefaultAsync(x =>
                    x.Id == outletId);

        if (outlet is null)
        {
            throw new InvalidOperationException(
                "المنفذ غير موجود.");
        }

        outlet.Name =
            request.Name.Trim();

        outlet.CompanyName =
            NormalizeOptional(
                request.CompanyName);

        outlet.Address =
            NormalizeOptional(
                request.Address);

        outlet.Phone =
            NormalizeOptional(
                request.Phone);

        outlet.TaxNumber =
            NormalizeOptional(
                request.TaxNumber);

        outlet.CurrencyCode =
            currencyCode;

        outlet.LogoUrl =
            NormalizeOptional(
                request.LogoUrl);

        await _context.SaveChangesAsync();

        return Map(outlet);
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static OutletSettingsDto Map(
        Domain.Entities.Outlet outlet)
    {
        return new OutletSettingsDto
        {
            Id =
                outlet.Id,

            Name =
                outlet.Name,

            CompanyName =
                outlet.CompanyName,

            Address =
                outlet.Address,

            Phone =
                outlet.Phone,

            TaxNumber =
                outlet.TaxNumber,

            CurrencyCode =
                outlet.CurrencyCode,

            LogoUrl =
                outlet.LogoUrl,

            IsActive =
                outlet.IsActive
        };
    }
}
