using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OutletControl.Web;
using OutletControl.Web.Auth;
using OutletControl.Web.Services;

var builder =
    WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl =
    builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiSettings:BaseUrl is not configured.");

if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

// ================================
// Authentication / Authorization
// ================================

builder.Services.AddScoped<AuthStorageService>();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<
    OutletControlAuthenticationStateProvider>();

builder.Services.AddScoped<
    AuthenticationStateProvider>(
        sp =>
            sp.GetRequiredService<
                OutletControlAuthenticationStateProvider>());

builder.Services.AddScoped<AuthMessageHandler>();

// ================================
// HttpClient
// ================================

builder.Services.AddScoped(sp =>
{
    var handler =
        sp.GetRequiredService<AuthMessageHandler>();

    handler.InnerHandler =
        new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress =
            new Uri(apiBaseUrl)
    };
});

// ================================
// API Services
// ================================

builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<CatalogApiService>();
builder.Services.AddScoped<SalesApiService>();
builder.Services.AddScoped<ShiftApiService>();
builder.Services.AddScoped<ReceivingApiService>();
builder.Services.AddScoped<DashboardApiService>();
builder.Services.AddScoped<InventoryApiService>();
builder.Services.AddScoped<SupplierAccountApiService>();
builder.Services.AddScoped<UploadApiService>();
builder.Services.AddScoped<TreasuryApiService>();
builder.Services.AddScoped<UserManagementApiService>();
builder.Services.AddScoped<OutletSettingsApiService>();


await builder.Build().RunAsync();