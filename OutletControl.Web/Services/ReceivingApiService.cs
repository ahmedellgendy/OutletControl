using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OutletControl.Contracts.Receiving;

namespace OutletControl.Web.Services;

public class ReceivingApiService
{
    private readonly HttpClient _httpClient;

    public ReceivingApiService(
        HttpClient httpClient)
    {
        _httpClient =
            httpClient;
    }

    public async Task<StockReceiptDto?>
        ReceiveAsync(
            ReceiveStockRequest request)
    {
        var response =
            await _httpClient
                .PostAsJsonAsync(
                    "api/receiving",
                    request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تسجيل استلام البضاعة."));
        }

        return await response.Content
            .ReadFromJsonAsync<
                StockReceiptDto>();
    }

    public async Task<
        IReadOnlyList<
            StockReceiptHistoryItemDto>>
        GetHistoryAsync(
            int outletId,
            DateTime? fromUtc = null,
            DateTime? toUtcExclusive = null)
    {
        var url =
            new StringBuilder(
                $"api/receiving/history/{outletId}");

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
                    "تعذر تحميل سجل الاستلامات."));
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    List<
                        StockReceiptHistoryItemDto>>();

        return result ??
               new List<
                   StockReceiptHistoryItemDto>();
    }

    public async Task<ReceivingExportFile>
        ExportReceiptAsync(
            int outletId,
            int receiptId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/receiving/export/{outletId}/{receiptId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تصدير فاتورة الاستلام."));
        }

        var content =
            await response.Content
                .ReadAsByteArrayAsync();

        var fileName =
            response.Content.Headers
                .ContentDisposition?
                .FileNameStar;

        if (string.IsNullOrWhiteSpace(
            fileName))
        {
            fileName =
                response.Content.Headers
                    .ContentDisposition?
                    .FileName?
                    .Trim('"');
        }

        if (string.IsNullOrWhiteSpace(
            fileName))
        {
            fileName =
                $"Receiving-{receiptId}.xlsx";
        }

        return new ReceivingExportFile
        {
            Content =
                content,

            FileName =
                fileName
        };
    }

    private static async Task<string>
        ReadApiErrorAsync(
            HttpResponseMessage response,
            string fallbackMessage)
    {
        try
        {
            var content =
                await response.Content
                    .ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(
                content))
            {
                return fallbackMessage;
            }

            using var document =
                JsonDocument.Parse(
                    content);

            if (document.RootElement
                .TryGetProperty(
                    "message",
                    out var messageElement))
            {
                var message =
                    messageElement
                        .GetString();

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

public class ReceivingExportFile
{
    public byte[] Content { get; set; } =
        Array.Empty<byte>();

    public string FileName { get; set; } =
        "Receiving.xlsx";
}