using System.Net.Http.Json;
using System.Text.Json;
using OutletControl.Contracts.Users;

namespace OutletControl.Web.Services;

public class UserManagementApiService
{
    private readonly HttpClient _httpClient;

    public UserManagementApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync()
    {
        var response =
            await _httpClient.GetAsync(
                "api/admin/users");

        await EnsureSuccessAsync(
            response,
            "تعذر تحميل المستخدمين.");

        return await response.Content
            .ReadFromJsonAsync<List<AdminUserDto>>()
            ?? new List<AdminUserDto>();
    }

    public async Task<AdminUserDto?> CreateUserAsync(
        CreateAdminUserRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/admin/users",
                request);

        await EnsureSuccessAsync(
            response,
            "تعذر إنشاء المستخدم.");

        return await response.Content
            .ReadFromJsonAsync<AdminUserDto>();
    }

    public async Task<AdminUserDto?> UpdateUserAsync(
        int userId,
        UpdateAdminUserRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/admin/users/{userId}",
                request);

        await EnsureSuccessAsync(
            response,
            "تعذر تحديث المستخدم.");

        return await response.Content
            .ReadFromJsonAsync<AdminUserDto>();
    }

    public async Task<AdminUserDto?> SetActiveAsync(
        int userId,
        bool isActive)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/admin/users/{userId}/active",
                new SetUserActiveRequest
                {
                    IsActive =
                        isActive
                });

        await EnsureSuccessAsync(
            response,
            "تعذر تحديث حالة المستخدم.");

        return await response.Content
            .ReadFromJsonAsync<AdminUserDto>();
    }

    public async Task ResetPasswordAsync(
        int userId,
        string newPassword)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"api/admin/users/{userId}/reset-password",
                new ResetUserPasswordRequest
                {
                    NewPassword =
                        newPassword
                });

        await EnsureSuccessAsync(
            response,
            "تعذر تغيير كلمة المرور.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string fallbackMessage)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message =
            await ReadApiErrorAsync(
                response,
                fallbackMessage);

        throw new InvalidOperationException(
            message);
    }

    private static async Task<string> ReadApiErrorAsync(
        HttpResponseMessage response,
        string fallbackMessage)
    {
        try
        {
            var content =
                await response.Content
                    .ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return fallbackMessage;

            using var document =
                JsonDocument.Parse(content);

            if (document.RootElement.TryGetProperty(
                "message",
                out var messageElement))
            {
                var message =
                    messageElement.GetString();

                if (!string.IsNullOrWhiteSpace(
                    message))
                {
                    return message;
                }
            }
        }
        catch
        {
        }

        return fallbackMessage;
    }
}
