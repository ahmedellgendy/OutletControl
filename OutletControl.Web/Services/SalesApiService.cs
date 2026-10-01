using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OutletControl.Contracts.Sales;

namespace OutletControl.Web.Services;

public class SalesApiService
{
    private readonly HttpClient _httpClient;

    public SalesApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SaleDto?> CreateSaleAsync(
        CreateSaleRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/sales",
                request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تسجيل عملية البيع."));
        }

        return await response.Content
            .ReadFromJsonAsync<SaleDto>();
    }

    public async Task<IReadOnlyList<SaleHistoryItemDto>> GetHistoryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null)
    {
        var url =
            new StringBuilder(
                $"api/sales/history/{outletId}");

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
                    "تعذر تحميل سجل المبيعات."));
        }

        return await response.Content
            .ReadFromJsonAsync<List<SaleHistoryItemDto>>()
            ?? new List<SaleHistoryItemDto>();
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

                if (!string.IsNullOrWhiteSpace(message))
                    return message;
            }
        }
        catch
        {
        }

        return fallbackMessage;
    }
}
