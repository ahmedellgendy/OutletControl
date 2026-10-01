using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OutletControl.Application.Catalog.Import;
using OutletControl.Application.Interfaces.Auth;
using OutletControl.Application.Interfaces.Catalog;
using OutletControl.Application.Interfaces.Dashboard;
using OutletControl.Application.Interfaces.Inventory;
using OutletControl.Application.Interfaces.Receiving;
using OutletControl.Application.Interfaces.Sales;
using OutletControl.Application.Interfaces.Shifts;
using OutletControl.Application.Interfaces.SupplierAccount;
using OutletControl.Application.Interfaces.Treasury;
using OutletControl.Application.Interfaces.Users;
using OutletControl.Infrastructure.Identity;
using OutletControl.Infrastructure.Persistence;
using OutletControl.Infrastructure.Services.Auth;
using OutletControl.Infrastructure.Services.Catalog;
using OutletControl.Infrastructure.Services.Dashboard;
using OutletControl.Infrastructure.Services.Inventory;
using OutletControl.Infrastructure.Services.Receiving;
using OutletControl.Infrastructure.Services.Sales;
using OutletControl.Infrastructure.Services.Shifts;
using OutletControl.Infrastructure.Services.SupplierAccount;
using OutletControl.Infrastructure.Services.Treasury;
using OutletControl.Infrastructure.Services.Users;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================================
// Database
// ================================

builder.Services.AddDbContext<OutletControlDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ================================
// Data Protection
// ================================

builder.Services.AddDataProtection();

// ================================
// Identity
// ================================

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<OutletControlDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),

                ClockSkew = TimeSpan.Zero
            };
    });

// ================================
// Application Services
// ================================

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IReceivingService, ReceivingService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISupplierAccountService, SupplierAccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<
    IProductImportService,
    ProductImportService>();
builder.Services.AddScoped<ITreasuryService, TreasuryService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<
    IReceivingExportService,
    ReceivingExportService>();

// ================================
// Controllers / OpenAPI
// ================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// ================================
// CORS
// ================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("OutletControlWeb", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

// ================================
// IMPORTANT
// Seeder متوقف مؤقتًا لحد ما نعمل
// Add-Migration + Update-Database
// ================================

 using (var scope = app.Services.CreateScope())
 {
     var userManager =
         scope.ServiceProvider
             .GetRequiredService<UserManager<ApplicationUser>>();

     var roleManager =
         scope.ServiceProvider
             .GetRequiredService<RoleManager<IdentityRole<int>>>();

     await IdentitySeeder.SeedAsync(
         userManager,
         roleManager);
 }

// ================================
// HTTP Pipeline
// ================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("OutletControlWeb");

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();