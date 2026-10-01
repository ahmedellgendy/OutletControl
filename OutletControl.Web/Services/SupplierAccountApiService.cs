using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OutletControl.Contracts.SupplierAccount;

namespace OutletControl.Web.Services;

public class SupplierAccountApiService
{
    private readonly HttpClient _httpClient;

    public SupplierAccountApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SupplierAccountDto?> GetAccountAsync(
        int outletId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/supplier-account/{outletId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تحميل حساب الشركة."));
        }

        return await response.Content
            .ReadFromJsonAsync<SupplierAccountDto>();
    }

    public async Task<SupplierAccountDto?> AddPaymentAsync(
        CreateSupplierPaymentRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/supplier-account/payments",
                request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تسجيل الدفعة."));
        }

        return await response.Content
            .ReadFromJsonAsync<SupplierAccountDto>();
    }

    public async Task<SupplierAccountLedgerDto?> GetLedgerAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null)
    {
        var url =
            new StringBuilder(
                $"api/supplier-account/{outletId}/ledger");

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
                    "تعذر تحميل حركات حساب الشركة."));
        }

        return await response.Content
            .ReadFromJsonAsync<SupplierAccountLedgerDto>();
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
