using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OutletControl.Contracts.Shifts;

namespace OutletControl.Web.Services;

public class ShiftApiService
{
    private readonly HttpClient _httpClient;

    public ShiftApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ShiftDto?> GetOpenShiftAsync(
        int outletId)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/shifts/open/{outletId}");

        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تحميل حالة اليوم."));
        }

        return await response.Content
            .ReadFromJsonAsync<ShiftDto>();
    }

    public async Task<ShiftDto?> OpenShiftAsync(
        OpenShiftRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/shifts/open",
                request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر فتح اليوم."));
        }

        return await response.Content
            .ReadFromJsonAsync<ShiftDto>();
    }

    public async Task<ShiftDto?> CloseShiftAsync(
        CloseShiftRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/shifts/close",
                request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر إغلاق اليوم."));
        }

        return await response.Content
            .ReadFromJsonAsync<ShiftDto>();
    }

    public async Task<IReadOnlyList<ShiftHistoryItemDto>> GetHistoryAsync(
        int outletId,
        DateTime? fromUtc = null,
        DateTime? toUtcExclusive = null,
        bool? isClosed = null)
    {
        var query = new StringBuilder(
            $"api/shifts/history/{outletId}");

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

        if (isClosed.HasValue)
        {
            parameters.Add(
                $"isClosed={isClosed.Value.ToString().ToLowerInvariant()}");
        }

        if (parameters.Count > 0)
        {
            query.Append('?');
            query.Append(
                string.Join("&", parameters));
        }

        var response =
            await _httpClient.GetAsync(
                query.ToString());

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                await ReadApiErrorAsync(
                    response,
                    "تعذر تحميل سجل الشيفتات."));
        }

        return await response.Content
            .ReadFromJsonAsync<List<ShiftHistoryItemDto>>()
            ?? new List<ShiftHistoryItemDto>();
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
