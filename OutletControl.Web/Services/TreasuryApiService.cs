using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OutletControl.Contracts.Treasury;

namespace OutletControl.Web.Services;

public class TreasuryApiService
{
    private readonly HttpClient _httpClient;

    public TreasuryApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TreasuryPageDto?> GetTreasuryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null)
    {
        var url =
            new StringBuilder(
                $"api/treasury/{outletId}");

        var parameters =
            new List<string>();

        if (fromUtc.HasValue)
        {
            parameters.Add(
                $"fromUtc={Uri.EscapeDataString(fromUtc.Value.ToString("O"))}");
        }

        if (toUtcExclusive.HasValue)
        {
            parameters.Add(
                $"toUtcExclusive={Uri.EscapeDataString(toUtcExclusive.Value.ToString("O"))}");
        }

        if (parameters.Count > 0)
        {
            url.Append('?');
            url.Append(
                string.Join(
                    "&",
                    parameters));
        }

        var response =
            await _httpClient.GetAsync(
                url.ToString());

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تحميل بيانات الخزنة."));
        }

        return await response.Content
            .ReadFromJsonAsync<TreasuryPageDto>();
    }

    public async Task<TreasuryMovementDto?> CreateMovementAsync(
        CreateTreasuryMovementRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/treasury/movements",
                request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تسجيل حركة الخزنة."));
        }

        return await response.Content
            .ReadFromJsonAsync<TreasuryMovementDto>();
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

            if (document.RootElement
                .TryGetProperty(
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
