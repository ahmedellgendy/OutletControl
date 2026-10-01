using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutletControl.Domain.Entities;
using OutletControl.Infrastructure.Identity;

namespace OutletControl.Infrastructure.Persistence;

public class OutletControlDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public OutletControlDbContext(
        DbContextOptions<OutletControlDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Outlet> Outlets => Set<Outlet>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockReceipt> StockReceipts => Set<StockReceipt>();
    public DbSet<StockReceiptItem> StockReceiptItems => Set<StockReceiptItem>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<TreasuryMovement> TreasuryMovements => Set<TreasuryMovement>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.SellingPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Outlet>(entity =>
        {
            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.CompanyName)
                .HasMaxLength(150);

            entity.Property(x => x.Address)
                .HasMaxLength(300);

            entity.Property(x => x.Phone)
                .HasMaxLength(50);

            entity.Property(x => x.TaxNumber)
                .HasMaxLength(100);

            entity.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(x => x.LogoUrl)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.OutletId,
                x.ProductId
            })
            .IsUnique();

            entity.Property(x => x.AverageUnitCost)
                .HasPrecision(18, 4);

            entity.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(x => x.Outlet)
                .WithMany(x => x.StockBalances)
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.HasOne(x => x.Outlet)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockReceipt>(entity =>
        {
            entity.Property(x => x.ReferenceNumber)
                .HasMaxLength(100);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Outlet)
                .WithMany()
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.StockReceipt)
                .HasForeignKey(x => x.StockReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.ClientReferenceId)
                 .HasMaxLength(64);

            entity.HasIndex(x => new
            {
                x.OutletId,
                x.ClientReferenceId
            })
            .IsUnique()
            .HasFilter("[ClientReferenceId] IS NOT NULL");
        });

        modelBuilder.Entity<StockReceiptItem>(entity =>
        {
            entity.Property(x => x.UnitCost)
                .HasPrecision(18, 4);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Outlet)
                .WithMany()
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Shift)
                .WithMany(x => x.Sales)
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Sale)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.ClientReferenceId)
                    .HasMaxLength(64);

                    entity.HasIndex(x => new
                    {
                        x.OutletId,
                        x.ClientReferenceId
                    })
                    .IsUnique()
                    .HasFilter("[ClientReferenceId] IS NOT NULL");
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.UnitCost)
                .HasPrecision(18, 4);

            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.Property(x => x.OpeningCash)
                .HasPrecision(18, 2);

            entity.Property(x => x.SalesAmount)
                .HasPrecision(18, 2);

            entity.Property(x => x.ExpectedCash)
                .HasPrecision(18, 2);

            entity.Property(x => x.ActualCash)
                .HasPrecision(18, 2);

            entity.Property(x => x.CashDifference)
                .HasPrecision(18, 2);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.HasOne(x => x.Outlet)
                .WithMany()
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.Property(x => x.Amount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.HasOne(x => x.Outlet)
                .WithMany()
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TreasuryMovement>(entity =>
        {
            entity.Property(x => x.Amount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Category)
                .HasMaxLength(100);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.ReferenceType)
                .HasMaxLength(50);

            entity.HasIndex(x => new
            {
                x.OutletId,
                x.OccurredAt
            });

            entity.HasIndex(x => new
            {
                x.ReferenceType,
                x.ReferenceId
            });

            entity.HasOne(x => x.Outlet)
                .WithMany()
                .HasForeignKey(x => x.OutletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Shift)
                .WithMany(x => x.TreasuryMovements)
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.OutletId);
        });

        modelBuilder.Entity<Outlet>().HasData(
            new Outlet
            {
                Id = 1,
                Name = "المنفذ الرئيسي",
                CompanyName = "Friday Ice Cream",
                Address = null,
                Phone = null,
                TaxNumber = null,
                CurrencyCode = "EGP",
                LogoUrl = null,
                IsActive = true
            });
    }
}
