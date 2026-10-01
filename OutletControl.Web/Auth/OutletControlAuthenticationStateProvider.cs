using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using OutletControl.Web.Services;

namespace OutletControl.Web.Auth;

public class OutletControlAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly AuthStorageService _authStorageService;

    public OutletControlAuthenticationStateProvider(
        AuthStorageService authStorageService)
    {
        _authStorageService =
            authStorageService;
    }

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        var token =
            await _authStorageService
                .GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous();
        }

        var principal =
            CreatePrincipal(token);

        if (principal is null)
        {
            await _authStorageService.ClearAsync();

            return Anonymous();
        }

        return new AuthenticationState(
            principal);
    }

    public async Task NotifyUserAuthenticationAsync(
        string token)
    {
        var principal =
            CreatePrincipal(token);

        if (principal is null)
        {
            await _authStorageService.ClearAsync();

            NotifyAuthenticationStateChanged(
                Task.FromResult(
                    Anonymous()));

            return;
        }

        await _authStorageService
            .SaveTokenAsync(token);

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(
                    principal)));
    }

    public async Task NotifyUserLogoutAsync()
    {
        await _authStorageService.ClearAsync();

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                Anonymous()));
    }

    private static ClaimsPrincipal? CreatePrincipal(
        string token)
    {
        try
        {
            var handler =
                new JwtSecurityTokenHandler();

            var jwt =
                handler.ReadJwtToken(token);

            if (jwt.ValidTo <=
                DateTime.UtcNow)
            {
                return null;
            }

            var claims =
                jwt.Claims.ToList();

            var identity =
                new ClaimsIdentity(
                    claims,
                    authenticationType: "jwt",
                    nameType: ClaimTypes.Name,
                    roleType: ClaimTypes.Role);

            return new ClaimsPrincipal(
                identity);
        }
        catch
        {
            return null;
        }
    }

    private static AuthenticationState Anonymous()
    {
        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity()));
    }
}