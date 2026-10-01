using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OutletControl.Web.Services;

public class UploadApiService
{
    private readonly HttpClient _httpClient;

    public UploadApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> UploadProductImageAsync(
        Stream stream,
        string fileName,
        string contentType)
    {
        using var content =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(stream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(contentType);

        content.Add(
            fileContent,
            "file",
            fileName);

        var response =
            await _httpClient.PostAsync(
                "api/uploads/product-image",
                content);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"تعذر رفع الصورة: {error}");
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<UploadImageResponse>();

        if (result is null ||
            string.IsNullOrWhiteSpace(result.ImageUrl))
        {
            throw new InvalidOperationException(
                "تم رفع الصورة ولكن لم يتم إرجاع رابط الصورة.");
        }

        return result.ImageUrl;
    }

    private sealed class UploadImageResponse
    {
        public string ImageUrl { get; set; }
            = string.Empty;
    }
}