using System.Net.Http.Json;
using System.Text.Json;
using OutletControl.Contracts.Inventory;

namespace OutletControl.Web.Services;

public class InventoryApiService
{
    private readonly HttpClient _httpClient;

    public InventoryApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<StockBalanceDto>>
        GetStockAsync(
            int outletId)
    {
        return await _httpClient
            .GetFromJsonAsync<List<StockBalanceDto>>(
                $"api/inventory/{outletId}")
            ?? new List<StockBalanceDto>();
    }

    public async Task<List<OpeningStockProductDto>>
        GetOpeningStockProductsAsync(
            int outletId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/inventory/opening-stock/products/{outletId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تحميل منتجات الرصيد الافتتاحي."));
        }

        return await response.Content
            .ReadFromJsonAsync<List<OpeningStockProductDto>>()
            ?? new List<OpeningStockProductDto>();
    }

    public async Task SetOpeningStockAsync(
        SetOpeningStockRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/inventory/opening-stock",
                request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر حفظ الرصيد الافتتاحي."));
        }
    }

    private static async Task<string>
        ReadApiErrorAsync(
            HttpResponseMessage response,
            string fallbackMessage)
    {
        var content =
            await response.Content
                .ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return fallbackMessage;
        }

        try
        {
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
        catch (JsonException)
        {
            // response is plain text
        }

        return content.Trim('"');
    }
}
