using System.Net.Http.Headers;

namespace OutletControl.Web.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly AuthStorageService _authStorageService;

    public AuthMessageHandler(
        AuthStorageService authStorageService)
    {
        _authStorageService = authStorageService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token =
            await _authStorageService.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}