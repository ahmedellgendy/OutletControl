using System.Net.Http.Json;
using OutletControl.Contracts.Auth;

namespace OutletControl.Web.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                "اسم المستخدم أو كلمة المرور غير صحيحة.");

        return await response.Content
            .ReadFromJsonAsync<LoginResponse>();
    }
}