using Microsoft.JSInterop;

namespace OutletControl.Web.Services;

public class AuthStorageService
{
    private const string TokenKey =
        "authToken";

    private readonly IJSRuntime _jsRuntime;

    public AuthStorageService(
        IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveTokenAsync(
        string token)
    {
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            TokenKey,
            token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            TokenKey);
    }

    public async Task RemoveTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            TokenKey);
    }

    public async Task ClearAsync()
    {
        await RemoveTokenAsync();

        // تنظيف القيم القديمة من النسخ السابقة.
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            "fullName");

        await _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            "role");

        await _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            "outletId");
    }
}