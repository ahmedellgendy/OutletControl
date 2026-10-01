using System.Net.Http.Json;
using System.Text.Json;
using OutletControl.Contracts.Outlets;

namespace OutletControl.Web.Services;

public class OutletSettingsApiService
{
    private readonly HttpClient _httpClient;

    public OutletSettingsApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OutletSettingsDto?> GetCurrentAsync()
    {
        var response =
            await _httpClient.GetAsync(
                "api/outlet-settings/current");

        await EnsureSuccessAsync(
            response,
            "تعذر تحميل إعدادات المنفذ.");

        return await response.Content
            .ReadFromJsonAsync<OutletSettingsDto>();
    }

    public async Task<OutletSettingsDto?> UpdateCurrentAsync(
        UpdateOutletSettingsRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                "api/outlet-settings/current",
                request);

        await EnsureSuccessAsync(
            response,
            "تعذر حفظ إعدادات المنفذ.");

        return await response.Content
            .ReadFromJsonAsync<OutletSettingsDto>();
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string fallbackMessage)
    {
        if (response.IsSuccessStatusCode)
            return;

        try
        {
            var json =
                await response.Content
                    .ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(json))
            {
                using var document =
                    JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty(
                    "message",
                    out var messageElement))
                {
                    var message =
                        messageElement.GetString();

                    if (!string.IsNullOrWhiteSpace(
                        message))
                    {
                        throw new InvalidOperationException(
                            message);
                    }
                }
            }
        }
        catch (JsonException)
        {
        }

        throw new InvalidOperationException(
            fallbackMessage);
    }
}
